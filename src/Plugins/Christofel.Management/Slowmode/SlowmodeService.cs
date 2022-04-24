//
//   SlowmodeService.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Extensions;
using Christofel.Helpers.Storages;
using Christofel.Management.Database;
using Christofel.Management.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.Management.Slowmode
{
    /// <summary>
    /// Service for registering, unregistering and handling disable of temporal slowmode.
    /// </summary>
    public class SlowmodeService
    {
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IDbContextFactory<ManagementContext> _dbContextFactory;
        private readonly ILogger _logger;
        private readonly IThreadSafeStorage<RegisteredTemporalSlowmode> _slowmodeStorage;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlowmodeService"/> class.
        /// </summary>
        /// <param name="dbContextFactory">The management database context factory.</param>
        /// <param name="channelApi">The channel api.</param>
        /// <param name="slowmodeStorage">The thread-safe storage of the temporal slowmodes.</param>
        /// <param name="logger">The logger.</param>
        public SlowmodeService
        (
            IDbContextFactory<ManagementContext> dbContextFactory,
            IDiscordRestChannelAPI channelApi,
            IThreadSafeStorage<RegisteredTemporalSlowmode> slowmodeStorage,
            ILogger<SlowmodeService> logger
        )
        {
            _logger = logger;
            _channelApi = channelApi;
            _dbContextFactory = dbContextFactory;
            _slowmodeStorage = slowmodeStorage;
        }

        /// <summary>
        /// Enables slowmode in given channel.
        /// </summary>
        /// <remarks>
        /// Disables all temporal slowmodes for the given channel, if there were any.
        /// </remarks>
        /// <param name="channelId">Id of the channel where to enable the slowmode.</param>
        /// <param name="interval">The rate of messages for the user.</param>
        /// <param name="ct">The cancellation token of the operation.</param>
        /// <returns>A result of the enable that may not succeed.</returns>
        public async Task<Result> EnableSlowmodeAsync
            (Snowflake channelId, TimeSpan interval, CancellationToken ct = default)
        {
            await UnregisterTemporalSlowmodeAsync(channelId, ct);

            var result = await _channelApi.ModifyChannelAsync
            (
                channelId,
                rateLimitPerUser: (int)interval.TotalSeconds,
                ct: ct
            );

            return result.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(result);
        }

        /// <summary>
        /// Disables slowmode in given channel.
        /// </summary>
        /// <remarks>
        /// Removes all temporal slowmodes for the given channel, if there were any.
        /// </remarks>
        /// <param name="channelId">Id of the channel where to enable the slowmode.</param>
        /// <param name="ct">The cancellation token of the operation.</param>
        /// <returns>A result of disable that may not succeed.</returns>
        public async Task<Result> DisableSlowmodeAsync(Snowflake channelId, CancellationToken ct)
        {
            await UnregisterTemporalSlowmodeAsync(channelId, ct);
            return await EnableSlowmodeAsync(channelId, TimeSpan.Zero, ct);
        }

        /// <summary>
        /// Cancels task of temporal slowmode and deletes it from the database.
        /// </summary>
        /// <param name="channelId">Id of the channel to disable temporal slowmode in.</param>
        /// <param name="ct">The cancellation token of the operation.</param>
        /// <returns>Whether there was any temporal slowmode that was canceled.</returns>
        public async Task<bool> UnregisterTemporalSlowmodeAsync(Snowflake channelId, CancellationToken ct = default)
        {
            await using var dbContext = _dbContextFactory.CreateDbContext();

            var matchingSlowmodes = _slowmodeStorage.Data
                .Where(x => x.TemporalSlowmodeEntity.ChannelId == channelId);

            var unregistered = false;
            foreach (var matchingSlowmode in matchingSlowmodes)
            {
                unregistered = true;
                matchingSlowmode.CancellationTokenSource.Cancel();
                _slowmodeStorage.Remove(matchingSlowmode);

                dbContext.Remove(matchingSlowmode.TemporalSlowmodeEntity);
            }

            if (unregistered)
            {
                await dbContext.SaveChangesAsync(ct);
            }

            return unregistered;
        }

        /// <summary>
        /// Register temporal slowmode task and save it to the database.
        /// </summary>
        /// <param name="channelId">Id of the channel where to register slowmode to.</param>
        /// <param name="userId">Id of the user who enabled the slowmode.</param>
        /// <param name="interval">The rate of messages for the user.</param>
        /// <param name="returnInterval">The rate of the messages to return to after the slowmode has passed.</param>
        /// <param name="duration">The duration after what the temporal slowmode will be removed.</param>
        /// <param name="ct">The cancellation token of the operation.</param>
        /// <returns>Information about the registered temporal slowmode.</returns>
        public async Task<RegisteredTemporalSlowmode> RegisterTemporalSlowmodeAsync
        (
            Snowflake channelId,
            Snowflake userId,
            TimeSpan interval,
            TimeSpan returnInterval,
            TimeSpan duration,
            CancellationToken ct = default
        )
        {
            await using var dbContext = _dbContextFactory.CreateDbContext();

            var temporalSlowmodeEntity = new TemporalSlowmode
            {
                ActivationDate = DateTime.Now,
                ChannelId = channelId,
                UserId = userId,
                DeactivationDate = DateTime.Now.Add(duration),
                Interval = interval,
                ReturnInterval = returnInterval,
            };

            dbContext.Add(temporalSlowmodeEntity);
            await dbContext.SaveChangesAsync(ct);

            return RegisterDisableHandler(temporalSlowmodeEntity);
        }

        /// <summary>
        /// Cancels all of the tasks that disable temporal slowmode.
        /// </summary>
        /// <returns>Number of the tasks canceled.</returns>
        public int CancelAllDisableHandlers()
        {
            var canceled = 0;
            foreach (var registeredTemporalSlowmode in _slowmodeStorage.Data)
            {
                canceled++;
                registeredTemporalSlowmode.CancellationTokenSource.Cancel();
            }

            return canceled;
        }

        /// <summary>
        /// Registers task for the temporal slowmode.
        /// </summary>
        /// <param name="temporalSlowmodeEntity">The entity that represents the slowmode to be registered.</param>
        /// <returns>Information about the registered slowmode.</returns>
        public RegisteredTemporalSlowmode RegisterDisableHandler(TemporalSlowmode temporalSlowmodeEntity)
        {
            var registeredTemporalSlowmode =
                new RegisteredTemporalSlowmode(temporalSlowmodeEntity, new CancellationTokenSource());
            _slowmodeStorage.Add(registeredTemporalSlowmode);

            var duration = temporalSlowmodeEntity.DeactivationDate - DateTime.Now;

            Task.Run
            (
                async () =>
                {
                    var canceled = false;
                    try
                    {
                        await Task.Delay(duration, registeredTemporalSlowmode.CancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        canceled = true;
                        _logger.LogDebug("Temporal slowmode disable was canceled");
                    }

                    if (!canceled)
                    {
                        var result = await EnableSlowmodeAsync
                        (
                            temporalSlowmodeEntity.ChannelId,
                            temporalSlowmodeEntity.ReturnInterval
                        ); // Cannot use cancellation token from registered slowmode, as that one will be canceled.

                        if (result.IsSuccess)
                        {
                            _logger.LogInformation
                            (
                                "Disabled temporal slowmode in channel <#{Channel}> enabled by <@{User}>. Returned to interval {ReturnInterval}",
                                temporalSlowmodeEntity.ChannelId,
                                temporalSlowmodeEntity.UserId,
                                temporalSlowmodeEntity.ReturnInterval
                            );
                        }
                        else
                        {
                            _logger.LogResultError
                            (
                                result,
                                $"Could not disable temporal slowmode in channel <#{temporalSlowmodeEntity.ChannelId}> enabled by <@{temporalSlowmodeEntity.UserId}>:"
                            );
                        }
                    }
                }
            );

            return registeredTemporalSlowmode;
        }
    }
}