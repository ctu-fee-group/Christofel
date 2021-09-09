using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Christofel.Plugins.Lifetime;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Core;
using Remora.Results;

namespace Christofel.Api.Ctu.JobQueue
{
    public record CtuAuthRoleAssign(
        Snowflake UserId,
        Snowflake GuildId,
        IReadOnlyList<Snowflake> AddRoles,
        IReadOnlyList<Snowflake> RemoveRoles,
        Action DoneCallback,
        int RetryCount = 0);

    /// <summary>
    /// Processor of role assigns, works on different thread
    /// </summary>
    /// <remarks>
    /// Creates thread only if there is job assigned,
    /// if there isn't, the thread is freed (thread pool is used)
    /// </remarks>
    public class CtuAuthRoleAssignProcessor : ThreadPoolJobQueue<CtuAuthRoleAssign>
    {
        private const int MaxRetryCount = 5;

        private readonly ILogger _logger;
        private readonly IDiscordRestGuildAPI _guildApi;
        private readonly ILifetime _pluginLifetime;

        public CtuAuthRoleAssignProcessor(ILogger<CtuAuthRoleAssignProcessor> logger,
            ICurrentPluginLifetime pluginLifetime, IDiscordRestGuildAPI guildApi)
            : base(pluginLifetime, logger)
        {
            _pluginLifetime = pluginLifetime;
            _logger = logger;
            _guildApi = guildApi;
        }
        
        protected override async Task ProcessAssignJob(CtuAuthRoleAssign assignJob)
        {
            bool error = false;
            var removeRoles = assignJob.RemoveRoles
                .Distinct();

            await HandleRolesEdit(assignJob, removeRoles, (roleId) => _guildApi
                .RemoveGuildMemberRoleAsync(assignJob.GuildId, assignJob.UserId, roleId,
                    "CTU Authentication", _pluginLifetime.Stopping));

            var assignRoles = assignJob.AddRoles
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
                EnqueueJob(new CtuAuthRoleAssign(assignJob.UserId, assignJob.GuildId,
                    assignJob.AddRoles, assignJob.RemoveRoles, assignJob.DoneCallback, assignJob.RetryCount + 1));

                EnqueueJob(assignJob with
                {
                    RetryCount = assignJob.RetryCount + 1
                });
                
                _logger.LogError(
                    $"Going to retry assigning roles to <@{assignJob.UserId}>.");
            }
            else if (error)
            {
                _logger.LogError(
                    $"Could not assign roles to user <@{assignJob.UserId}> and maximal number of retries was reached. Roles to add: {string.Join(",", assignJob.AddRoles.Select(x => x.Value))}, Roles to remove: {string.Join(", ", assignJob.RemoveRoles.Select(x => x.Value))}.");
                assignJob.DoneCallback();
            }
            else
            {
                _logger.LogInformation($"Successfully added roles to <@{assignJob.UserId}>");
                assignJob.DoneCallback();
            }
        }
    }
}