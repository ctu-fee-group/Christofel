//
//   CtuAuthRoleAssignProcessor.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Christofel.BaseLib.Extensions;
using Christofel.Helpers.JobQueue;
using Christofel.Plugins.Lifetime;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.Api.Ctu.JobQueue
{
    /// <summary>
    /// Processor of role assigns, works on different thread.
    /// </summary>
    /// <remarks>
    /// Creates thread only if there is job assigned,
    /// if there isn't, the thread is freed (thread pool is used).
    /// </remarks>
    public class CtuAuthRoleAssignProcessor : ThreadPoolJobQueue<CtuAuthRoleAssign>
    {
        private const int MaxRetryCount = 5;
        private readonly IDiscordRestGuildAPI _guildApi;

        private readonly ILogger _logger;
        private readonly ILifetime _pluginLifetime;

        /// <summary>
        /// Initializes a new instance of the <see cref="CtuAuthRoleAssignProcessor"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="pluginLifetime">The lifetime of the current plugin.</param>
        /// <param name="guildApi">The guild api.</param>
        public CtuAuthRoleAssignProcessor
        (
            ILogger<CtuAuthRoleAssignProcessor> logger,
            ICurrentPluginLifetime pluginLifetime,
            IDiscordRestGuildAPI guildApi
        )
            : base(pluginLifetime, logger)
        {
            _pluginLifetime = pluginLifetime;
            _logger = logger;
            _guildApi = guildApi;
        }

        /// <inheritdoc />
        protected override async Task ProcessAssignJob(CtuAuthRoleAssign assignJob)
        {
            var error = false;
            var removeRoles = assignJob.RemoveRoles
                .Distinct();

            await HandleRolesEdit
            (
                assignJob,
                removeRoles,
                roleId => _guildApi
                    .RemoveGuildMemberRoleAsync
                    (
                        assignJob.GuildId,
                        assignJob.UserId,
                        roleId,
                        "CTU Authentication",
                        _pluginLifetime.Stopping
                    )
            );

            var assignRoles = assignJob.AddRoles
                .Distinct();

            await HandleRolesEdit
            (
                assignJob,
                assignRoles,
                roleId => _guildApi
                    .AddGuildMemberRoleAsync
                    (
                        assignJob.GuildId,
                        assignJob.UserId,
                        roleId,
                        "CTU Authentication",
                        _pluginLifetime.Stopping
                    )
            );

            HandleResult(error, assignJob);
        }

        private async Task<bool> HandleRolesEdit
        (
            CtuAuthRoleAssign assignJob,
            IEnumerable<Snowflake> roleIds,
            EditRole editFunction
        )
        {
            var error = false;
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
                    _logger.LogResultError
                    (
                        result,
                        $"Couldn't add or remove role <@&{roleId}> from user <@{assignJob.UserId}>"
                    );
                }
            }

            return error;
        }

        private void HandleResult(bool error, CtuAuthRoleAssign assignJob)
        {
            if (error && assignJob.RetryCount < MaxRetryCount)
            {
                EnqueueJob
                (
                    new CtuAuthRoleAssign
                    (
                        assignJob.UserId,
                        assignJob.GuildId,
                        assignJob.AddRoles,
                        assignJob.RemoveRoles,
                        assignJob.DoneCallback,
                        assignJob.RetryCount + 1
                    )
                );

                EnqueueJob(assignJob with { RetryCount = assignJob.RetryCount + 1 });

                _logger.LogError($"Going to retry assigning roles to <@{assignJob.UserId}>.");
            }
            else if (error)
            {
                _logger.LogError
                (
                    $"Could not assign roles to user <@{assignJob.UserId}> and maximal number of retries was reached. Roles to add: {string.Join(",", assignJob.AddRoles.Select(x => x.Value))}, Roles to remove: {string.Join(", ", assignJob.RemoveRoles.Select(x => x.Value))}."
                );
                assignJob.DoneCallback();
            }
            else
            {
                _logger.LogInformation($"Successfully added roles to <@{assignJob.UserId}>");
                assignJob.DoneCallback();
            }
        }

        private delegate Task<Result> EditRole(Snowflake roleId);
    }

    /// <summary>
    /// The job for <see cref="Christofel.Api.Ctu.JobQueue.CtuAuthRoleAssignProcessor"/>.
    /// </summary>
    /// <param name="UserId">The id of the user to assign roles to.</param>
    /// <param name="GuildId">The guild id of the user.</param>
    /// <param name="AddRoles">The roles to be added.</param>
    /// <param name="RemoveRoles">The roles to be removed.</param>
    /// <param name="DoneCallback">The callback to be called when the job is finished.</param>
    /// <param name="RetryCount">The count of maximal retries.</param>
    public record CtuAuthRoleAssign
    (
        Snowflake UserId,
        Snowflake GuildId,
        IReadOnlyList<Snowflake> AddRoles,
        IReadOnlyList<Snowflake> RemoveRoles,
        Action DoneCallback,
        int RetryCount = 0
    );
}