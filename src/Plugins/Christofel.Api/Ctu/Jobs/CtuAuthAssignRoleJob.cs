//
//  CtuAuthAssignRoleJob.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Scheduling;
using Christofel.Scheduling.Recoverable;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Core;
using Remora.Results;

namespace Christofel.Api.Ctu.Jobs
{
    /// <summary>
    /// Job that assigns or removes roles on the guild for the specified user.
    /// </summary>
    public class CtuAuthAssignRoleJob : IDataJob<CtuAuthRoleAssign>
    {
        private readonly IDiscordRestGuildAPI _guildApi;
        private readonly ILogger<CtuAuthAssignRoleJob> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CtuAuthAssignRoleJob"/> class.
        /// </summary>
        /// <param name="data">The entity that is associated with this entity.</param>
        /// <param name="guildApi">The guild api.</param>
        /// <param name="logger">The logger.</param>
        public CtuAuthAssignRoleJob
            (CtuAuthRoleAssign data, IDiscordRestGuildAPI guildApi, ILogger<CtuAuthAssignRoleJob> logger)
        {
            _logger = logger;
            _guildApi = guildApi;
            _logger = logger;
            Data = data;
        }

        /// <inheritdoc />
        public CtuAuthRoleAssign Data { get; set; }

        /// <inheritdoc />
        public async Task<Result> ExecuteAsync(IJobContext jobContext, CancellationToken ct = default)
        {
            var removeRoles = Data.RemoveRoles
                .Distinct();

            var errors = new List<IResult>();

            var removeResult = await HandleRolesEdit
            (
                Data,
                removeRoles,
                roleId => _guildApi
                    .RemoveGuildMemberRoleAsync
                    (
                        Data.GuildId,
                        Data.UserId,
                        roleId,
                        "CTU Authentication",
                        ct
                    ),
                ct
            );

            if (!removeResult.IsSuccess)
            {
                errors.Add(removeResult);
            }

            var assignRoles = Data.AddRoles
                .Distinct();

            var assignResult = await HandleRolesEdit
            (
                Data,
                assignRoles,
                roleId => _guildApi
                    .AddGuildMemberRoleAsync
                    (
                        Data.GuildId,
                        Data.UserId,
                        roleId,
                        "CTU Authentication",
                        ct
                    ),
                ct
            );

            if (!assignResult.IsSuccess)
            {
                errors.Add(assignResult);
            }

            return HandleResult
            (
                errors.Count switch
                {
                    1 => errors[0],
                    > 1 => (Result)new AggregateError(errors),
                    _ => Result.FromSuccess(),
                },
                Data
            );
        }

        private async Task<Result> HandleRolesEdit
        (
            CtuAuthRoleAssign assignJob,
            IEnumerable<Snowflake> roleIds,
            EditRole editFunction,
            CancellationToken ct
        )
        {
            var errors = new List<IResult>();
            foreach (var roleId in roleIds)
            {
                if (ct.IsCancellationRequested)
                {
                    _logger.LogWarning("Could not finish roles assignments");
                    ct.ThrowIfCancellationRequested();
                }

                var result = await editFunction(roleId);

                if (!result.IsSuccess)
                {
                    errors.Add(result);
                    _logger.LogError
                    (
                        $"Couldn't add or remove role <@&{roleId}> from user <@{assignJob.UserId}>: {result.Error.Message}"
                    );
                }
            }

            return errors.Count > 0
                ? new AggregateError(errors)
                : Result.FromSuccess();
        }

        private Result HandleResult(IResult result, CtuAuthRoleAssign assignJob)
        {
            if (!result.IsSuccess)
            {
                _logger.LogError
                (
                    "Could not assign roles to user <@{User}. {Error}",
                    assignJob.UserId,
                    result.Error?.Message
                );
            }
            else
            {
                _logger.LogInformation($"Successfully added roles to <@{assignJob.UserId}>");
            }

            return result.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(result.Error ?? new InvalidOperationError());
        }

        private delegate Task<Result> EditRole(Snowflake roleId);
    }
}