//
//   MessageCommandsGroup.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Christofel.CommandsLib.Permissions;
using Christofel.CommandsLib.Validator;
using Christofel.Helpers.Storages;
using Christofel.Management.Slowmode;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Core;
using Remora.Results;

namespace Christofel.Management.Commands
{
    /// <summary>
    /// The command group for /slowmode commands.
    /// </summary>
    [Group("slowmode")]
    [RequirePermission("management.slowmode")]
    [Description("Manage slowmode in a channel")]
    [DiscordDefaultPermission(false)]
    [Ephemeral]
    public class MessageCommandsGroup : CommandGroup
    {
        private readonly ICommandContext _context;
        private readonly FeedbackService _feedbackService;
        private readonly ILogger<MessageCommandsGroup> _logger;
        private readonly SlowmodeService _slowmodeService;
        private readonly IThreadSafeStorage<RegisteredTemporalSlowmode> _slowmodeStorage;
        private readonly IDiscordRestChannelAPI _channelApi;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageCommandsGroup"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="feedbackService">The feedback service.</param>
        /// <param name="context">The context of the current command.</param>
        /// <param name="slowmodeService">The service used for managing slowmodes.</param>
        /// <param name="slowmodeStorage">The storage for the slowmodes.</param>
        /// <param name="channelApi">The channel api.</param>
        public MessageCommandsGroup
        (
            ILogger<MessageCommandsGroup> logger,
            FeedbackService feedbackService,
            ICommandContext context,
            SlowmodeService slowmodeService,
            IThreadSafeStorage<RegisteredTemporalSlowmode> slowmodeStorage,
            IDiscordRestChannelAPI channelApi
        )
        {
            _slowmodeStorage = slowmodeStorage;
            _slowmodeService = slowmodeService;
            _logger = logger;
            _context = context;
            _feedbackService = feedbackService;
            _channelApi = channelApi;
        }

        /// <summary>
        /// Handles /slowmode for.
        /// </summary>
        /// <remarks>
        /// Registers task for temporal slowmode for given channel
        /// and enable the slowmode with message rate of <paramref name="interval"/>.
        ///
        /// The task will disable the slowmode after the specified duration
        /// by returning to the specified <paramref name="returnInterval"/> message rate.
        /// If <paramref name="returnInterval"/> is not specified, the channel's current
        /// slowmode interval value will be used.
        /// </remarks>
        /// <param name="interval">The message rate for users.</param>
        /// <param name="duration">The duration of the temporal slowmode.</param>
        /// <param name="returnInterval">The message rate for the users to return to after the temporal slowmode has passed.</param>
        /// <param name="channel">The channel to enable temporal slowmode in.</param>
        /// <returns>A result that may not have succeeded.</returns>
        [Command("for")]
        [RequirePermission("management.slowmode.for")]
        [Description("Enables slowmode for specified duration (hours, minutes, seconds)")]
        public async Task<Result> HandleSlowmodeFor
        (
            [Description("Rate limit per user (formatted time 3m, 3m20s, 1h20m etc.). Maximum is 6 hours.")]
            TimeSpan interval,
            [Description
                ("How long should the slowmode be enabled for (formatted time 3m, 3m20s etc.). Maximum is 48 hours.")]
            TimeSpan duration,
            [Description
            (
                "Interval to return to after the temporal slowmode is disabled. By default, the old one will be used."
            )]
            TimeSpan? returnInterval = null,
            [Description("Channel to enable slowmode in. Current channel if omitted.")]
            [DiscordTypeHint(TypeHint.Channel)]
            Snowflake? channel = null
        )
        {
            var validationResult = new CommandValidator()
                .MakeSure("interval", interval.TotalHours, o => o.GreaterThan(0).LessThanOrEqualTo(6))
                .MakeSure("returnInterval", returnInterval?.TotalHours ?? 1, o => o.GreaterThan(0).LessThanOrEqualTo(6))
                .MakeSure("totalSeconds", duration.TotalHours, o => o.GreaterThan(0).LessThanOrEqualTo(48))
                .Validate()
                .GetResult();

            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            var channelId = channel ?? _context.ChannelID;

            if (returnInterval is null)
            {
                var channelResult = await _channelApi.GetChannelAsync(channelId, CancellationToken);

                if (!channelResult.IsSuccess)
                {
                    return Result.FromError(channelResult);
                }

                if (channelResult.Entity.RateLimitPerUser.IsDefined(out var setReturnInterval))
                {
                    returnInterval = setReturnInterval;
                }
                else
                {
                    returnInterval = TimeSpan.Zero;
                }
            }

            var enableResult = await _slowmodeService.EnableSlowmodeAsync(channelId, interval, CancellationToken);

            if (!enableResult.IsSuccess)
            {
                await _feedbackService.SendContextualErrorAsync
                    ($"Could not enable slowmode in channel <#{channelId}>: {enableResult.Error.Message}");
                return enableResult;
            }

            RegisteredTemporalSlowmode temporalSlowmode;
            try
            {
                temporalSlowmode = await _slowmodeService.RegisterTemporalSlowmodeAsync
                    (channelId, _context.User.ID, interval, (TimeSpan)returnInterval, duration);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not register temporal slowmode");
                await _feedbackService.SendContextualErrorAsync("Could not register temporal slowmode handler");
                await _slowmodeService.DisableSlowmodeAsync(channelId, CancellationToken);

                return new ExceptionError(e);
            }

            var feedbackResult = await _feedbackService.SendContextualSuccessAsync
            (
                $"Enabled slowmode in channel <#{channelId}> for {duration} (until {temporalSlowmode.TemporalSlowmodeEntity.DeactivationDate}). The return slowmode interval will be {returnInterval}. Use this command again for changing the return interval.",
                ct: CancellationToken
            );

            return feedbackResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(feedbackResult);
        }

        /// <summary>
        /// Handles /slowmode show.
        /// </summary>
        /// <remarks>
        /// Shows all active registered temporal slowmodes.
        /// </remarks>
        /// <returns>A result that may not have succeeded.</returns>
        [Command("show")]
        [RequirePermission("management.slowmode.show")]
        [Description("Shows what temporal slowmodes are currently enabled")]
        public async Task<IResult> HandleSlowmodeShow()
        {
            var channels = _slowmodeStorage.Data.Select
                (
                    x =>
                        $"- <#{x.TemporalSlowmodeEntity.ChannelId}> - expires <t:{((DateTimeOffset)x.TemporalSlowmodeEntity.DeactivationDate).ToUnixTimeSeconds()}:R> with rate limit of {x.TemporalSlowmodeEntity.Interval}"
                )
                .ToList();

            if (channels.Count == 0)
            {
                return await _feedbackService.SendContextualInfoAsync
                    ("There are currently no temporal slowmodes enabled.");
            }

            var message = "Temporal slowmodes are currently enabled in following channels: ";
            return await _feedbackService.SendContextualInfoAsync(message + "\n" + string.Join("\n", channels));
        }

        /// <summary>
        /// Handles /slowmode enable.
        /// </summary>
        /// <remarks>
        /// Enables permanent slowmode in the given channel.
        /// </remarks>
        /// <param name="interval">The message rate for users.</param>
        /// <param name="channel">The channel where to enable the slowmode.</param>
        /// <returns>A result that may not have succeeded.</returns>
        [Command("enable")]
        [RequirePermission("management.slowmode.enable")]
        [Description("Enables slowmode **permanently** in specified channel")]
        public async Task<IResult> HandleSlowmodeEnable
        (
            [Description("Rate limit per user (formatted time 3m, 3m20s, 1h20m etc.). Maximum is 6 hours.")]
            TimeSpan interval,
            [Description("Channel to enable slowmode in. Current channel if omitted.")]
            [DiscordTypeHint(TypeHint.Channel)]
            Optional<Snowflake> channel = default
        )
        {
            var validationResult = new CommandValidator()
                .MakeSure("interval", interval.TotalHours, o => o.GreaterThan(0).LessThanOrEqualTo(6))
                .Validate()
                .GetResult();

            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            var channelId = channel.HasValue
                ? channel.Value
                : _context.ChannelID;

            var result = await _slowmodeService.EnableSlowmodeAsync(channelId, interval, CancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogError($"Could not enable slowmode in channel <#{channelId}>: {result.Error?.Message}");
                await _feedbackService.SendContextualErrorAsync
                (
                    "Something has gone wrong",
                    ct: CancellationToken
                );
                return result;
            }

            return await _feedbackService.SendContextualSuccessAsync("Slowmode enabled", ct: CancellationToken);
        }

        /// <summary>
        /// Handles /slowmode disable.
        /// </summary>
        /// <remarks>
        /// Disables slowmode in the given channel.
        /// </remarks>
        /// <param name="channel">The channel where to disable slowmode.</param>
        /// <returns>A result that may not have succeeded.</returns>
        [Command("disable")]
        [RequirePermission("management.slowmode.disable")]
        [Description("Disables slowmode in specified channel")]
        public async Task<IResult> HandleSlowmodeDisable
        (
            [Description("Channel to disable slowmode in. Current channel if omitted.")]
            [DiscordTypeHint(TypeHint.Channel)]
            Snowflake? channel = null
        )
        {
            var channelId = channel ?? _context.ChannelID;
            var result = await _slowmodeService.DisableSlowmodeAsync(channelId, CancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogError($"Could not disable slowmode in channel <#{channelId}>: {result.Error?.Message}");
                await _feedbackService.SendContextualErrorAsync
                    ($"Could not disable slowmode in channel <#{channelId}>: {result.Error?.Message}.");
                return result;
            }

            return await _feedbackService.SendContextualInfoAsync
                ($"Slowmode in channel <#{channelId}> successfully disabled.");
        }
    }
}