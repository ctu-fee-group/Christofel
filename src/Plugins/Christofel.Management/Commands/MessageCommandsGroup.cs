using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Implementations.Storages;
using Christofel.CommandsLib;
using Christofel.CommandsLib.Extensions;
using Christofel.CommandsLib.Validator;
using Christofel.Management.Slowmode;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Core;
using Remora.Results;

namespace Christofel.Management.Commands
{
    [Group("slowmode")]
    [RequirePermission("management.slowmode")]
    [Description("Manage slowmode in a channel")]
    [DiscordDefaultPermission(false)]
    [Ephemeral]
    public class MessageCommandsGroup : CommandGroup
    {
        private readonly ILogger<MessageCommandsGroup> _logger;
        private readonly FeedbackService _feedbackService;
        private readonly ICommandContext _context;
        private readonly SlowmodeService _slowmodeService;
        private readonly IThreadSafeStorage<RegisteredTemporalSlowmode> _slowmodeStorage;

        public MessageCommandsGroup(ILogger<MessageCommandsGroup> logger,
            FeedbackService feedbackService, ICommandContext context, SlowmodeService slowmodeService,
            IThreadSafeStorage<RegisteredTemporalSlowmode> slowmodeStorage)
        {
            _slowmodeStorage = slowmodeStorage;
            _slowmodeService = slowmodeService;
            _logger = logger;
            _context = context;
            _feedbackService = feedbackService;
        }

        [Command("for")]
        [RequirePermission("management.slowmode.for")]
        [Description("Enables slowmode for specified duration (hours, minutes, seconds)")]
        public async Task<Result> HandleSlowmodeFor(
            [Description("Rate limit per user (formatted time 3m, 3m20s, 1h20m etc.). Maximum is 6 hours.")]
            TimeSpan interval,
            [Description(
                "How long should the slowmode be enabled for (formatted time 3m, 3m20s etc.). Maximum is 48 hours.")]
            TimeSpan duration,
            [Description("Channel to enable slowmode in. Current channel if omitted."),
             DiscordTypeHint(TypeHint.Channel)]
            Snowflake? channel = null)
        {
            var validationResult = new CommandValidator()
                .MakeSure("interval", interval.TotalHours, o => o.GreaterThan(0).LessThanOrEqualTo(6))
                .MakeSure("totalSeconds", duration.TotalHours, o => o.GreaterThan(0).LessThanOrEqualTo(48))
                .Validate()
                .GetResult();

            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            var channelId = channel ?? _context.ChannelID;
            
            var enableResult = await _slowmodeService.EnableSlowmodeAsync(channelId, interval, CancellationToken);

            if (!enableResult.IsSuccess)
            {
                await _feedbackService.SendContextualErrorAsync($"Could not enable slowmode in channel <#{channelId}>: {enableResult.Error.Message}");
                return enableResult;
            }

            RegisteredTemporalSlowmode temporalSlowmode;
            try
            {
                temporalSlowmode = await _slowmodeService.RegisterTemporalSlowmodeAsync(channelId, _context.User.ID, interval, duration);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not register temporal slowmode");
                await _feedbackService.SendContextualErrorAsync("Could not register temporal slowmode handler");
                await _slowmodeService.DisableSlowmodeAsync(channelId, CancellationToken);

                return new ExceptionError(e);
            }

            var feedbackResult = await _feedbackService.SendContextualSuccessAsync(
                $"Enabled slowmode in channel <#{channelId}> for {duration} (until {temporalSlowmode.TemporalSlowmodeEntity.DeactivationDate})",
                ct: CancellationToken);

            return feedbackResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(feedbackResult);
        }

        [Command("show")]
        [RequirePermission("management.slowmode.show")]
        [Description("Shows what temporal slowmodes are currently enabled")]
        public async Task<IResult> HandleSlowmodeShow()
        {
            var channels = _slowmodeStorage.Data.Select(x =>
                $"- <#{x.TemporalSlowmodeEntity.ChannelId}> - expires <t:{((DateTimeOffset)(x.TemporalSlowmodeEntity.DeactivationDate)).ToUnixTimeSeconds()}:R> with rate limit of {x.TemporalSlowmodeEntity.Interval}")
                .ToList();

            if (channels.Count == 0)
            {
                return await _feedbackService.SendContextualInfoAsync("There are currently no temporal slowmodes enabled.");
            }
            
            var message = "Temporal slowmodes are currently enabled in following channels: ";
            return await _feedbackService.SendContextualInfoAsync(message + "\n" + string.Join("\n", channels));
        }

        [Command("enable")]
        [RequirePermission("management.slowmode.enable")]
        [Description("Enables slowmode **permanently** in specified channel")]
        public async Task<IResult> HandleSlowmodeEnable(
            [Description("Rate limit per user (formatted time 3m, 3m20s, 1h20m etc.). Maximum is 6 hours.")]
            TimeSpan interval,
            [Description("Channel to enable slowmode in. Current channel if omitted."),
             DiscordTypeHint(TypeHint.Channel)]
            Optional<Snowflake> channel = default)
        {
            var validationResult = new CommandValidator()
                .MakeSure("interval", interval.TotalHours, o => o.GreaterThan(0).LessThanOrEqualTo(6))
                .Validate()
                .GetResult();

            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            var channelId = channel.HasValue ? channel.Value : _context.ChannelID;

            var result = await _slowmodeService.EnableSlowmodeAsync(channelId, interval, CancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogError($"Could not enable slowmode in channel <#{channelId}>: {result.Error?.Message}");
                await _feedbackService.SendContextualErrorAsync("Something has gone wrong",
                    ct: CancellationToken);
                return result;
            }

            return await _feedbackService.SendContextualSuccessAsync("Slowmode enabled", ct: CancellationToken);
        }

        [Command("disable")]
        [RequirePermission("management.slowmode.disable")]
        [Description("Disables slowmode in specified channel")]
        public async Task<IResult> HandleSlowmodeDisable(
            [Description("Channel to disable slowmode in. Current channel if omitted."),
             DiscordTypeHint(TypeHint.Channel)]
            Snowflake? channel = null)
        {
            var channelId = channel ?? _context.ChannelID;
            var result = await _slowmodeService.DisableSlowmodeAsync(channelId, CancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogError($"Could not disable slowmode in channel <#{channelId}>: {result.Error?.Message}");
                await _feedbackService.SendContextualErrorAsync($"Could not disable slowmode in channel <#{channelId}>: {result.Error?.Message}.");
                return result;
            }

            return await _feedbackService.SendContextualInfoAsync($"Slowmode in channel <#{channelId}> successfully disabled.");
        }
    }
}