using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Christofel.Api.Ctu.Database;
using Christofel.BaseLib.Lifetime;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Core;
using Remora.Results;

namespace Christofel.Api.Ctu
{
    /// <summary>
    /// Processor of role assigns, works on different thread
    /// </summary>
    /// <remarks>
    /// Creates thread only if there is job assigned,
    /// if there isn't, the thread is freed (thread pool is used)
    /// </remarks>
    public class CtuAuthRoleAssignProcessor
    {
        private const int MaxRetryCount = 5;

        private record CtuAuthRoleAssign(IGuildMember GuildMember, Snowflake UserId, Snowflake GuildId,
            List<CtuAuthRole> AddRoles, List<CtuAuthRole> RemoveRoles, int RetryCount);

        private readonly object _queueLock = new object();
        private readonly Queue<CtuAuthRoleAssign> _queue;

        private bool _threadRunning;

        private ICurrentPluginLifetime _pluginLifetime;

        private readonly ILogger _logger;
        private readonly IDiscordRestGuildAPI _guildApi;

        public CtuAuthRoleAssignProcessor(ILogger<CtuAuthRoleAssignProcessor> logger,
            ICurrentPluginLifetime pluginLifetime, IDiscordRestGuildAPI guildApi)
        {
            _logger = logger;
            _guildApi = guildApi;
            _queue = new Queue<CtuAuthRoleAssign>();
            _pluginLifetime = pluginLifetime;
        }

        public void EnqueueAssignJob(IGuildMember guildMember, Snowflake userId, Snowflake guildId,
            List<CtuAuthRole> addRoles, List<CtuAuthRole> removeRoles)
        {
            EnqueueAssignJob(new CtuAuthRoleAssign(guildMember, userId, guildId, addRoles, removeRoles, 0));
        }

        private void EnqueueAssignJob(CtuAuthRoleAssign assignJob)
        {
            if (_pluginLifetime.Stopping.IsCancellationRequested)
            {
                _logger.LogWarning("Cannot enqueue more assignment jobs as application is stopping.");
                return;
            }
            
            bool createThread = false;
            lock (_queueLock)
            {
                _queue.Enqueue(assignJob);

                if (!_threadRunning)
                {
                    createThread = true;
                    _threadRunning = true;
                    Task.Run(ProcessQueue);
                }
            }

            if (createThread)
            {
                _logger.LogDebug("Creating new job thread");
            }
        }

        private async Task ProcessQueue()
        {
            bool shouldRun = true;
            while (shouldRun)
            {
                try
                {
                    CtuAuthRoleAssign assignJob;

                    if (_pluginLifetime.Stopping.IsCancellationRequested)
                    {
                        _logger.LogWarning("Could not finish roles assignments");
                        return;
                    }

                    lock (_queueLock)
                    {
                        assignJob = _queue.Dequeue();

                        if (_queue.Count == 0)
                        {
                            shouldRun = false;
                            _threadRunning = false;
                        }
                    }

                    await ProcessAssignJob(assignJob);
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, "Role assign job has thrown an exception.");
                }
            }

            _logger.LogDebug("Destroying job thread, because no job is queued");
        }

        private async Task ProcessAssignJob(CtuAuthRoleAssign assignJob)
        {
            bool error = false;
            var removeRoles = assignJob.RemoveRoles
                .Select(x => new Snowflake(x.RoleId))
                .Distinct();

            await HandleRolesEdit(assignJob, removeRoles, (roleId) => _guildApi
                .RemoveGuildMemberRoleAsync(assignJob.GuildId, assignJob.UserId, roleId,
                    "CTU Authentication", _pluginLifetime.Stopping));

            var assignRoles = assignJob.AddRoles
                .Select(x => new Snowflake(x.RoleId))
                .Except(assignJob.GuildMember.Roles)
                .Distinct();

            await HandleRolesEdit(assignJob, assignRoles, (roleId) => _guildApi
                .AddGuildMemberRoleAsync(assignJob.GuildId, assignJob.UserId, roleId,
                    "CTU Authentication", _pluginLifetime.Stopping));
            
            HandleResult(error, assignJob);
        }

        private delegate Task<Result> EditRole(Snowflake roleId);

        private async Task<bool> HandleRolesEdit(CtuAuthRoleAssign assignJob, IEnumerable<Snowflake> roleIds,
            EditRole editFunction)
        {
            bool error = false;
            foreach (var roleId in roleIds)
            {
                if (_pluginLifetime.Stopping.IsCancellationRequested)
                {
                    _logger.LogWarning("Could not finish roles assignments");
                    return error;
                }

                var result = await editFunction(roleId);

                if (!result.IsSuccess)
                {
                    error = true;
                    _logger.LogError(
                        $"Couldn't add or remove role <@&{roleId}> from user <@{assignJob.UserId}>: {result.Error.Message}");
                }
            }

            return error;
        }

        private void HandleResult(bool error, CtuAuthRoleAssign assignJob)
        {
            if (error && assignJob.RetryCount < MaxRetryCount)
            {
                EnqueueAssignJob(new CtuAuthRoleAssign(assignJob.GuildMember, assignJob.UserId, assignJob.GuildId,
                    assignJob.AddRoles, assignJob.RemoveRoles, assignJob.RetryCount + 1));

                _logger.LogError(
                    $"Going to retry assigning roles to <@{assignJob.UserId}>. Roles to add: {string.Join(",", assignJob.AddRoles.Select(x => x.RoleId))}, Roles to remove: {string.Join(", ", assignJob.RemoveRoles.Select(x => x.RoleId))}.");
            }
            else if (error)
            {
                _logger.LogError(
                    $"Could not assign roles to user <@{assignJob.UserId}> and maximal number of retries was reached.");
            }
            else
            {
                _logger.LogInformation($"Successfully added roles to <@{assignJob.UserId}>");
            }
        }
    }
}