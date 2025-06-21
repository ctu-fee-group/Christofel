//
//   SelfManagementCommands.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Christofel.CommandsLib.Permissions;
using Christofel.CommandsLib.Validator;
using Christofel.Helpers.Date;
using Christofel.Helpers.Errors;
using Christofel.Helpers.Localization;
using Christofel.Management;
using Christofel.Management.Errors;
using FluentValidation;
using Microsoft.Extensions.Options;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Extensions.Formatting;
using Remora.Results;

/// <summary>
/// A class for commands applied only to self, ie. /selftimeout.
/// This means these commands can be used even by non-moderators.
/// </summary>
[RequirePermission("management.selfmanagement")]
public class SelfManagementCommands : CommandGroup
{
    private readonly IOperationContext _context;
    private readonly FeedbackService _feedback;
    private readonly IDiscordRestGuildAPI _guildApi;
    private readonly LocalizedStringLocalizer<ManagementPlugin> _localizer;
    private readonly IDateTimeProvider _dateTimeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelfManagementCommands"/> class.
    /// </summary>
    /// <param name="context">Context the command is executed in.</param>
    /// <param name="feedback">The feedback service.</param>
    /// <param name="guildApi">The discord guild api.</param>
    /// <param name="dateTimeProvider">The date time provider.</param>
    /// <param name="localizer">The localizer for localizing textual user messages.</param>
    public SelfManagementCommands(
        IOperationContext context,
        FeedbackService feedback,
        IDiscordRestGuildAPI guildApi,
        IDateTimeProvider dateTimeProvider,
        LocalizedStringLocalizer<ManagementPlugin> localizer
    )
    {
        _context = context;
        _feedback = feedback;
        _guildApi = guildApi;
        _localizer = localizer;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <summary>
    /// Specification used in selftimeoutuntil command.
    /// </summary>
    public enum TimeoutUntilSpecification
    {
        /// <summary>
        /// Till the end of today. (midnight).
        /// </summary>
        EndOfDay,

        /// <summary>
        /// Till the tomorrow's morning - 6 AM.
        /// </summary>
        TomorrowMorning,

        /// <summary>
        /// Till the end of week (start of week + 7 days).
        /// </summary>
        EndOfWeek,

        /// <summary>
        /// Till the end of work week (start of week + 5 days).
        /// </summary>
        EndOfWorkWeek,

        /// <summary>
        /// Till the end of current month.
        /// </summary>
        EndOfMonth,
    }

    private DateTimeOffset SpecificationToDateTimeOffset(TimeoutUntilSpecification specification)
    {
        var now = _dateTimeProvider.PreferredNow;
        var today = now.Date;

        int dayOfWeek = ((int)today.DayOfWeek + 6) % 7; // start with monday
        var startOfWeek = today.AddDays(-dayOfWeek);
        var startOfMonth = today.AddDays(-(int)today.Day);
        switch (specification)
        {
            case TimeoutUntilSpecification.EndOfDay:
                return today.AddDays(1);
            case TimeoutUntilSpecification.TomorrowMorning:
                return today.AddDays(1).AddHours(6);
            case TimeoutUntilSpecification.EndOfWeek:
                return startOfWeek.AddDays(7);
            case TimeoutUntilSpecification.EndOfWorkWeek:
                var endOfWorkWeek = startOfWeek.AddDays(5);

                // already past Friday, next week.
                if (endOfWorkWeek < now)
                {
                    endOfWorkWeek = endOfWorkWeek.AddDays(7);
                }

                return endOfWorkWeek;
            case TimeoutUntilSpecification.EndOfMonth:
                return startOfMonth.AddDays(DateTime.DaysInMonth(startOfMonth.Year, startOfMonth.Month));
        }

        throw new UnreachableException();
    }

    /// <summary>
    /// Like <see cref="HandleSelfTimeoutAsync"/>, but calculates commonly requested
    /// times for timeout durations.
    /// </summary>
    /// <param name="until">The specification that says when the timeout expires, out of common enum values.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [Command("selftimeoutuntil")]
    [Description("Timeout self until given time like rest of day.")]
    [RequirePermission("management.selfmanagement.selftimeout")]
    public async Task<IResult> HandleSelfTimeoutUntil
        (
            [Description("When the timeout should end.")]
            TimeoutUntilSpecification until
        )
    {
        DateTimeOffset timeoutUntil = SpecificationToDateTimeOffset(until);

        if (timeoutUntil < _dateTimeProvider.UtcNow)
        {
            return await _feedback.SendContextualErrorAsync("The specified time has already passed.");
        }

        // Maximum reached, round.
        if ((timeoutUntil - _dateTimeProvider.UtcNow).TotalDays > 28)
        {
            timeoutUntil = _dateTimeProvider.UtcNow.AddDays(28);
        }

        return await SelfTimeout(timeoutUntil);
    }

    /// <summary>
    /// The user assigns themselves a timeout for arbitrary duration from 1s to 28d (maximum supported by Discord).
    /// </summary>
    /// <param name="duration">The duration to timeout for.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [Command("selftimeout")]
    [RequirePermission("management.selfmanagement.selftimeout")]
    [Description("Timeout self for given duration. Supports units: w - weeks, d - days, h - hours, m - mins, s - secs")]
    public async Task<IResult> HandleSelfTimeoutAsync(
        [Description("Formatted duration of the timeout, ie. 1h30m, 1d20h30m10s")] TimeSpan duration
    )
    {
        // 1. validate lower than 28 days (maximum), greater than 0
        var validationResult = new CommandValidator()
            .MakeSure("interval", duration.TotalDays, o => o.GreaterThan(0).LessThanOrEqualTo(28))
            .Validate()
            .GetResult();

        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        DateTimeOffset timeoutUntil = DateTime.Now + duration;

        return await SelfTimeout(timeoutUntil);
    }

    private string FormatTimeSpan(TimeSpan span)
    {
        var formatted = new StringBuilder();

        if (span.Days > 0)
        {
            formatted.Append($"{span.Days}d ");
        }
        if (span.Hours > 0)
        {
            formatted.Append($"{span.Hours}h ");
        }
        if (span.Minutes > 0)
        {
            formatted.Append($"{span.Minutes}m ");
        }
        if (span.Seconds > 0)
        {
            formatted.Append($"{span.Seconds}s");
        }

        return formatted.ToString();
    }

    private async Task<IResult> SelfTimeout(DateTimeOffset timeoutUntil)
    {
        if (!_context.TryGetUserID(out var userId))
        {
            // Error intentionally ignored.
            await _feedback.SendContextualErrorAsync(
                "Couldn't find your user id, this is a bug. Aborting.",
                ct: CancellationToken);
            return Result.FromError(
                new UnexpectedContextError(
                    nameof(HandleSelfTimeoutAsync),
                    "UserID"));
        }

        // 2. Load the guild for channel where the command has been issued
        if (!_context.TryGetGuildID(out var guildId))
        {
            // Error intentionally ignored.
            await _feedback.SendContextualErrorAsync(
                "It seems that you're not executing this command in a guild. The /selftimeout command works only in guilds.",
                ct: CancellationToken);
            return Result.FromError(
                new UnexpectedContextError(
                    nameof(HandleSelfTimeoutAsync),
                    "GuildID"));
        }

        // 3. Check the user doesn't have timeout in this guild.
        // If they do, abort
        // This is a sanity check. This should't really be possible - the user cannot use commands when they have timeout, right?
        var memberResult = await _guildApi.GetGuildMemberAsync(guildId, userId, ct: CancellationToken);

        if (!memberResult.IsDefined(out var member))
        {
            // Error intentionally ignored.
            await _feedback.SendContextualErrorAsync(
                "Couldn't retrieve information about you. Aborting.", ct: CancellationToken);
            return memberResult;
        }

        if (member.CommunicationDisabledUntil.IsDefined(out var currentTimeoutUntil)
            && currentTimeoutUntil > DateTime.Now)
        {
            // Error intentionally ignored.
            await _feedback.SendContextualErrorAsync("You already do have a timeout, aborting.");
            return Result.FromError(
                new SelfManagementError(nameof(HandleSelfTimeoutAsync), "Already has timeout"));
        }

        // 4. Give them timeout for the given duration
        var duration = TimeSpan.FromSeconds(Math.Round((timeoutUntil - DateTime.Now).TotalSeconds));

        var result = await _guildApi.ModifyGuildMemberAsync
            (
                guildId,
                userId,
                communicationDisabledUntil: timeoutUntil,
                reason: "Self-timeout",
                ct: CancellationToken
            );

        if (!result.IsSuccess)
        {
            // Error intentionally ignored.
            await _feedback.SendContextualErrorAsync(
                "There was an error when setting the timeout.",
                ct: CancellationToken);
            return result;
        }

        // Print: The user has assigned themselves timeout for {duration} until {timeoutUntil}
        return await _feedback.SendContextualSuccessAsync(
            _localizer.Translate(
                "SELFTIMEOUT_SUCCESSFUL",
                $"<@{userId}>",
                Markdown.Timestamp(timeoutUntil, TimestampStyle.RelativeTime),
                Markdown.Timestamp(timeoutUntil, TimestampStyle.ShortDateTime)));
    }
}
