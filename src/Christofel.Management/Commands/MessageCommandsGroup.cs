using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Christofel.CommandsLib;
using Christofel.CommandsLib.Extensions;
using Christofel.CommandsLib.Validator;
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
        // /slowmode for interval hours minutes seconds channel
        // /slowmode enablepermanent interval channel
        // /slowmode disable channel
        private readonly ILogger<MessageCommandsGroup> _logger;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly FeedbackService _feedbackService;
        private readonly ICommandContext _context;

        public MessageCommandsGroup(ILogger<MessageCommandsGroup> logger, IDiscordRestChannelAPI channelApi,
            FeedbackService feedbackService, ICommandContext context)
        {
            _logger = logger;
            _context = context;
            _channelApi = channelApi;
            _feedbackService = feedbackService;
        }

        [Command("for")]
        [RequirePermission("management.slowmode.for")]
        [Description("Enables slowmode for specified duration (hours, minutes, seconds)")]
        public async Task<IResult> HandleSlowmodeFor(int interval, long? hours, long? minutes,
            long? seconds, [DiscordTypeHint(TypeHint.Channel)] Optional<Snowflake> channel)
        {
            long totalSeconds = ((hours ?? 0) * 60 + (minutes ?? 0)) * 60 + (seconds ?? 0);

            var validationResult = new CommandValidator()
                .MakeSure("interval", interval, o => o.InclusiveBetween(1, 3600))
                .MakeSure("totalSeconds", totalSeconds, o => o.GreaterThan(0))
                .Validate()
                .GetResult();

            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            return await _feedbackService.SendContextualErrorAsync("Not implemented", ct: CancellationToken);
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
                return await _feedbackService.SendContextualErrorAsync("Something has gone wrong",
                    ct: CancellationToken);
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