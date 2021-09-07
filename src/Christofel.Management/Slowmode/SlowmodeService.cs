using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Implementations.Storages;
using Christofel.Management.Database;
using Christofel.Management.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Core;
using Remora.Results;

namespace Christofel.Management.Slowmode
{
    public class SlowmodeService
    {
        private readonly IThreadSafeStorage<RegisteredTemporalSlowmode> _slowmodeStorage;
        private readonly IDbContextFactory<ManagementContext> _dbContextFactory;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly ILogger _logger;

        public SlowmodeService(IDbContextFactory<ManagementContext> dbContextFactory, IDiscordRestChannelAPI channelApi,
            IThreadSafeStorage<RegisteredTemporalSlowmode> slowmodeStorage, ILogger<SlowmodeService> logger)
        {
            _logger = logger;
            _channelApi = channelApi;
            _dbContextFactory = dbContextFactory;
            _slowmodeStorage = slowmodeStorage;
        }

        public async Task<Result> EnableSlowmodeAsync(Snowflake channelId, TimeSpan interval, CancellationToken ct = default)
        {
            await UnregisterTemporalSlowmodeAsync(channelId);

            var result = await _channelApi.ModifyChannelAsync(channelId, rateLimitPerUser: (int)interval.TotalSeconds,
                ct: ct);

            return result.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(result);
        }

        public async Task<Result> DisableSlowmodeAsync(Snowflake channelId, CancellationToken ct)
        {
            await UnregisterTemporalSlowmodeAsync(channelId);
            return await EnableSlowmodeAsync(channelId, TimeSpan.Zero, ct);
        }

        public async Task<bool> UnregisterTemporalSlowmodeAsync(Snowflake channelId, CancellationToken ct = default)
        {
            await using var dbContext = _dbContextFactory.CreateDbContext();

            var matchingSlowmodes = _slowmodeStorage.Data
                .Where(x => x.TemporalSlowmodeEntity.ChannelId == channelId);

            bool unregistered = false;
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

        public async Task<RegisteredTemporalSlowmode> RegisterTemporalSlowmodeAsync(Snowflake channelId, TimeSpan interval,
            TimeSpan duration, CancellationToken ct = default)
        {
            await using var dbContext = _dbContextFactory.CreateDbContext();

            var temporalSlowmodeEntity = new TemporalSlowmode()
            {
                ActivationDate = DateTime.Now,
                ChannelId = channelId,
                DeactivationDate = DateTime.Now.Add(duration),
                Interval = interval
            };

            dbContext.Add(temporalSlowmodeEntity);
            await dbContext.SaveChangesAsync(ct);

            return RegisterDisableHandler(temporalSlowmodeEntity);
        }

        public int CancelAllDisableHandlers()
        {
            int canceled = 0;
            foreach (var registeredTemporalSlowmode in _slowmodeStorage.Data)
            {
                canceled++;
                registeredTemporalSlowmode.CancellationTokenSource.Cancel();
            }

            return canceled;
        }

        public RegisteredTemporalSlowmode RegisterDisableHandler(TemporalSlowmode temporalSlowmodeEntity)
        {
            var registeredTemporalSlowmode =
                new RegisteredTemporalSlowmode(temporalSlowmodeEntity, new CancellationTokenSource());
            _slowmodeStorage.Add(registeredTemporalSlowmode);
            
            TimeSpan duration = temporalSlowmodeEntity.ActivationDate - DateTime.Now;

            Task.Run(async () =>
            {
                bool canceled = false;
                try
                {
                    await Task.Delay(duration, registeredTemporalSlowmode.CancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    canceled = true;
                    _logger.LogDebug("Temporal slowmode disable was canceled");
                }

                if (canceled)
                {
                    var result = await DisableSlowmodeAsync(registeredTemporalSlowmode.TemporalSlowmodeEntity.ChannelId,
                        default); // Cannot use cancellation token from registered slowmode, as that one will be canceled.

                    if (result.IsSuccess)
                    {
                        _logger.LogInformation(
                            "Disabled temporal slowmode in channel <#{Channel}> enabled by <@{User}>",
                            temporalSlowmodeEntity.ChannelId,
                            temporalSlowmodeEntity.UserId);
                    }
                    else
                    {
                        _logger.LogError("Could not disable temporal slowmode in channel <#{Channel}> enabled by <@{User}>: {Error}",
                            temporalSlowmodeEntity.ChannelId,
                            temporalSlowmodeEntity.UserId,
                            result.Error.Message);
                    }
                }
            });

            return registeredTemporalSlowmode;
        }
    }
}