//
//   SlowmodeDisableJob.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Christofel.Management.Database.Models;
using Christofel.Scheduling;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.Management.Slowmode
{
    /// <summary>
    /// Disables temporal slowmode in the given channel.
    /// </summary>
    public class SlowmodeDisableJob : IJob
    {
        private readonly ILogger _logger;
        private readonly SlowmodeService _slowmodeService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlowmodeDisableJob"/> class.
        /// </summary>
        /// <param name="temporalSlowmode">The slowmode entity that should be disabled.</param>
        /// <param name="slowmodeService">The service for slowmode.</param>
        /// <param name="logger">The logger.</param>
        public SlowmodeDisableJob
            (TemporalSlowmode temporalSlowmode, SlowmodeService slowmodeService, ILogger<SlowmodeDisableJob> logger)
        {
            _slowmodeService = slowmodeService;
            _logger = logger;
            TemporalSlowmode = temporalSlowmode;
        }

        /// <summary>
        /// Gets temporal slowmode entity.
        /// </summary>
        public TemporalSlowmode TemporalSlowmode { get; }

        /// <inheritdoc />
        public async Task<Result> ExecuteAsync(IJobContext jobContext, CancellationToken ct = default)
        {
            var result = await _slowmodeService.EnableSlowmodeAsync
            (
                TemporalSlowmode.ChannelId,
                TemporalSlowmode.ReturnInterval,
                ct
            ); // Cannot use cancellation token from registered slowmode, as that one will be canceled.

            if (result.IsSuccess)
            {
                _logger.LogInformation
                (
                    "Disabled temporal slowmode in channel <#{Channel}> enabled by <@{User}>. Returned to interval {ReturnInterval}",
                    TemporalSlowmode.ChannelId,
                    TemporalSlowmode.UserId,
                    TemporalSlowmode.ReturnInterval
                );
            }
            else
            {
                _logger.LogError
                (
                    "Could not disable temporal slowmode in channel <#{Channel}> enabled by <@{User}>: {Error}",
                    TemporalSlowmode.ChannelId,
                    TemporalSlowmode.UserId,
                    result.Error.Message
                );
            }

            return Result.FromSuccess();
        }
    }
}
