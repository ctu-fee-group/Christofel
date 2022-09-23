//
//   CtuAuthProcess.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu.Auth;
using Christofel.Api.Ctu.Auth.Conditions;
using Christofel.Api.Ctu.Auth.Steps;
using Christofel.Api.Ctu.Auth.Tasks;
using Christofel.BaseLib.Extensions;
using Christofel.Common.Database;
using Christofel.Common.Database.Models;
using Christofel.Common.User;
using Christofel.OAuth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remora.Commands;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Rest.Core;
using Remora.Results;
using Constants = Remora.Discord.API.Constants;

namespace Christofel.Api.Ctu
{
    /// <summary>
    /// Used to iterate through all steps of auth process.
    /// </summary>
    public class CtuAuthProcess
    {
        private readonly ILogger<CtuAuthProcess> _logger;

        private readonly IServiceProvider _services;

        /// <summary>
        /// Initializes a new instance of the <see cref="CtuAuthProcess"/> class.
        /// </summary>
        /// <param name="services">The provider of the services.</param>
        /// <param name="logger">The logger.</param>
        public CtuAuthProcess
        (
            IServiceProvider services,
            ILogger<CtuAuthProcess> logger
        )
        {
            _logger = logger;
            _services = services;
        }

        /// <summary>
        /// Proceed to do all the steps
        /// If step fails, exception will be thrown.
        /// </summary>
        /// <param name="accessToken">Valid token that can be used for Kos and Usermap.</param>
        /// <param name="ctuOauthHandler">The ctu oauth handler.</param>
        /// <param name="dbContext">Context monitoring dbUser.</param>
        /// <param name="guildId">Id of the guild we are workikng in.</param>
        /// <param name="dbUser">Database user to be edited and saved.</param>
        /// <param name="guildUser">Discord user used for auth purposes. Should be user with the id of dbUser.</param>
        /// <param name="ct">Cancellation token in case the request is cancelled.</param>
        /// <returns>A result that may not have succeeded.</returns>
        public async Task<Result> FinishAuthAsync
        (
            string accessToken,
            ICtuTokenApi ctuOauthHandler,
            ChristofelBaseContext dbContext,
            ulong guildId,
            DbUser dbUser,
            IGuildMember guildUser,
            CancellationToken ct = default
        )
        {
            using var scope = _services.CreateScope();
            var services = scope.ServiceProvider;
            services.GetRequiredService<ICtuTokenProvider>().AccessToken = accessToken;

            // 1. set ctu username
            ICtuUser loadedUser;
            try
            {
                loadedUser = await ctuOauthHandler.CheckTokenAsync(accessToken, ct);
            }
            catch (Exception e)
            {
                return e;
            }

            CtuAuthProcessData authData = new CtuAuthProcessData
            (
                accessToken,
                new LinkUser(0, loadedUser.CtuUsername, dbUser.DiscordId),
                new Snowflake(guildId, Constants.DiscordEpoch),
                dbContext,
                dbUser,
                guildUser,
                new CtuAuthAssignedRoles()
            );

            // 3. run conditions (if any failed, abort)
            var conditionsResult = await ExecuteConditionsAsync(services, authData, ct);

            dbUser.CtuUsername ??= loadedUser.CtuUsername;
            var databaseResult = await SaveToDatabase(dbContext, authData, ct);

            if (!conditionsResult.IsSuccess)
            {
                return conditionsResult;
            }

            // Save filled username to the database
            if (!databaseResult.IsSuccess)
            {
                return databaseResult;
            }

            // 4. run steps (if any failed, abort)
            var stepsResult = await ExecuteStepsAsync(services, authData, ct);
            if (!stepsResult.IsSuccess)
            {
                return stepsResult;
            }

            // 5. save to database
            databaseResult = await SaveToDatabase(dbContext, authData, ct);
            if (!databaseResult.IsSuccess)
            {
                return databaseResult;
            }

            // 6. run tasks (if any failed, log it, but continue)
            var tasksResult = await ExecuteTasks(services, authData, ct);
            if (!tasksResult.IsSuccess)
            {
                return new SoftAuthError(tasksResult.Error);
            }

            return tasksResult;
        }

        private async Task<Result> ExecuteConditionsAsync
        (
            IServiceProvider services,
            IAuthData authData,
            CancellationToken ct = default
        )
        {
            var conditions = services
                .GetServices<IPreAuthCondition>();

            foreach (var condition in conditions)
            {
                try
                {
                    var conditionResult = await condition.CheckPreAsync(authData, ct);
                    if (!conditionResult.IsSuccess)
                    {
                        return conditionResult;
                    }
                }
                catch (Exception e)
                {
                    return e;
                }
            }

            return Result.FromSuccess();
        }

        private async Task<Result> ExecuteStepsAsync
        (
            IServiceProvider services,
            IAuthData authData,
            CancellationToken ct = default
        )
        {
            var steps = services
                .GetServices<IAuthStep>();

            foreach (var step in steps)
            {
                try
                {
                    var stepResult = await step.FillDataAsync(authData, ct);
                    if (!stepResult.IsSuccess)
                    {
                        return stepResult;
                    }
                }
                catch (Exception e)
                {
                    return e;
                }
            }

            return Result.FromSuccess();
        }

        private async Task<Result> ExecuteTasks
        (
            IServiceProvider services,
            IAuthData authData,
            CancellationToken ct = default
        )
        {
            var tasks = services
                .GetServices<IAuthTask>();

            var errors = new List<IResult>();
            foreach (var task in tasks)
            {
                Result taskResult;
                try
                {
                    taskResult = await task.ExecuteAsync(authData, ct);
                }
                catch (Exception e)
                {
                    taskResult = e;
                }

                if (!taskResult.IsSuccess)
                {
                    errors.Add(taskResult);
                    _logger.LogResultError(taskResult, "Could not finish auth task");
                }
            }

            return errors.Count > 0
                ? new AggregateError(errors)
                : Result.FromSuccess();
        }

        private async Task<Result> SaveToDatabase
        (
            ChristofelBaseContext dbContext,
            IAuthData data,
            CancellationToken ct = default
        )
        {
            try
            {
                await dbContext.SaveChangesAsync(ct);
                return Result.FromSuccess();
            }
            catch (Exception e)
            {
                _logger.LogError
                (
                    e,
                    $"Database context save changes has thrown an exception while saving user data ({data.DbUser.UserId} {data.DbUser.DiscordId} {data.DbUser.CtuUsername})"
                );
                return e;
            }
        }

        private record LinkUser(int UserId, string CtuUsername, Snowflake DiscordId) : ILinkUser;
    }
}