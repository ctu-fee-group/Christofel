using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu.Auth.Conditions;
using Christofel.Api.Ctu.Auth.Steps;
using Christofel.Api.Ctu.Auth.Tasks;
using Christofel.Api.OAuth;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.User;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Commands.Services;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Results;

namespace Christofel.Api.Ctu
{
    /// <summary>
    /// Used to iterate through all steps of auth process
    /// </summary>
    public class CtuAuthProcess
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<CtuAuthProcess> _logger;

        private readonly TypeRepository<IPreAuthCondition> _conditionRepository;
        private readonly TypeRepository<IAuthTask> _taskRepository;
        private readonly TypeRepository<IAuthStep> _stepRepository;

        /// <summary>
        /// Initialize CtuAuthProcess
        /// </summary>
        public CtuAuthProcess(
            IServiceProvider services,
            ILogger<CtuAuthProcess> logger,
            IOptions<TypeRepository<IPreAuthCondition>> conditionRepository,
            IOptions<TypeRepository<IAuthTask>> taskRepository,
            IOptions<TypeRepository<IAuthStep>> stepRepository
        )
        {
            _logger = logger;
            _services = services;
            _conditionRepository = conditionRepository.Value;
            _taskRepository = taskRepository.Value;
            _stepRepository = stepRepository.Value;
        }

        /// <summary>
        /// Proceed to do all the steps
        /// If step fails, exception will be thrown
        /// </summary>
        /// <param name="accessToken">Valid token that can be used for Kos and Usermap</param>
        /// <param name="ctuOauthHandler"></param>
        /// <param name="dbContext">Context monitoring dbUser</param>
        /// <param name="guildId">Id of the guild we are workikng in</param>
        /// <param name="dbUser">Database user to be edited and saved</param>
        /// <param name="guildUser">Discord user used for auth purposes. Should be user with the id of dbUser</param>
        /// <param name="ct">Cancellation token in case the request is cancelled</param>
        public async Task<Result> FinishAuthAsync(string accessToken, CtuOauthHandler ctuOauthHandler,
            ChristofelBaseContext dbContext, ulong guildId, DbUser dbUser,
            IGuildMember guildUser, CancellationToken ct = default)
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
            
            CtuAuthProcessData authData = new CtuAuthProcessData(
                accessToken,
                loadedUser,
                guildId,
                dbContext,
                dbUser,
                guildUser,
                new CtuAuthAssignedRoles(),
                new Dictionary<string, object>()
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
            if (!conditionsResult.IsSuccess)
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
            return await ExecuteTasks(services, authData, ct);
        }

        private async Task<Result> ExecuteConditionsAsync(IServiceProvider services, IAuthData authData,
            CancellationToken ct = default)
        {
            var conditionTasks = _conditionRepository
                .GetTypes<IPreAuthCondition>()
                .Select(services.GetRequiredService)
                .Cast<IPreAuthCondition>()
                .Select(x => x.CheckPreAsync(authData, ct));

            foreach (var conditionTask in conditionTasks)
            {
                try
                {
                    var conditionResult = await conditionTask;
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

        private async Task<Result> ExecuteStepsAsync(IServiceProvider services, IAuthData authData,
            CancellationToken ct = default)
        {
            var stepTasks = _stepRepository
                .GetTypes<IAuthStep>()
                .Select(services.GetRequiredService)
                .Cast<IAuthStep>()
                .Select(x => x.FillDataAsync(authData, ct));

            foreach (var stepTask in stepTasks)
            {
                try
                {
                    var stepResult = await stepTask;
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

        private async Task<Result> ExecuteTasks(IServiceProvider services, IAuthData authData,
            CancellationToken ct = default)
        {
            var tasks = _taskRepository
                .GetTypes<IAuthTask>()
                .Select(services.GetRequiredService)
                .Cast<IAuthTask>()
                .Select(x => x.ExecuteAsync(authData, ct));

            bool error = false;
            foreach (var task in tasks)
            {
                Result taskResult;
                try
                {
                    taskResult = await task;
                }
                catch (Exception e)
                {
                    taskResult = e;
                }
                
                if (!taskResult.IsSuccess)
                {
                    error = true;
                    _logger.LogError($"Could not finish auth task: {taskResult.Error.Message}");
                }
            }

            return error 
                ? new GenericError("Could not finish tasks execution successfully")
                : Result.FromSuccess();
        }

        private async Task<Result> SaveToDatabase(ChristofelBaseContext dbContext, IAuthData data,
            CancellationToken ct = default)
        {
            try
            {
                await dbContext.SaveChangesAsync(ct);
                return Result.FromSuccess();
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    $"Database context save changes has thrown an exception while saving user data ({data.DbUser.UserId} {data.DbUser.DiscordId} {data.DbUser.CtuUsername})");
                return e;
            }
        }
    }
}