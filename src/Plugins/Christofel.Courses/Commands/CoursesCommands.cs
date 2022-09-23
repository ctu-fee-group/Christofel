//
//   CoursesCommands.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Linq;
using Christofel.BaseLib.Extensions;
using Christofel.CommandsLib.Permissions;
using Christofel.Common.Database;
using Christofel.Common.Database.Models;
using Christofel.Courses.Data;
using Christofel.CoursesLib.Data;
using Christofel.CoursesLib.Services;
using Microsoft.EntityFrameworkCore;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

namespace Christofel.Courses.Commands;

/// <summary>
/// A class for /courses command group.
/// </summary>
[Group("courses")]
[RequirePermission("courses.courses")]
[Ephemeral]
public class CoursesCommands : CommandGroup
{
    private readonly FeedbackService _feedbackService;
    private readonly ICommandContext _commandContext;
    private readonly CoursesChannelAssigner _channelAssigner;
    private readonly IReadableDbContext<ChristofelBaseContext> _baseContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoursesCommands"/> class.
    /// </summary>
    /// <param name="feedbackService">The feedback service.</param>
    /// <param name="commandContext">The command context.</param>
    /// <param name="channelAssigner">The courses channel assigner.</param>
    /// <param name="baseContext">The readable christofel base database context.</param>
    public CoursesCommands
    (
        FeedbackService feedbackService,
        ICommandContext commandContext,
        CoursesChannelAssigner channelAssigner,
        IReadableDbContext<ChristofelBaseContext> baseContext
    )
    {
        _feedbackService = feedbackService;
        _commandContext = commandContext;
        _channelAssigner = channelAssigner;
        _baseContext = baseContext;
    }

    /// <summary>
    /// Join the given courses.
    /// </summary>
    /// <param name="courses">The courses to join separated by space.</param>
    /// <returns>A result that may or may not be successful.</returns>
    [Command("join")]
    [Description("Join channels matching the given courses.")]
    [RequirePermission("courses.join")]
    public async Task<IResult> JoinAsync([Description("Courses to join separated by space")] string courses)
    {
        var discordUser = new DiscordUser(_commandContext.User.ID);

        var coursesAssignmentResult = await _channelAssigner.AssignCourses
            (discordUser, courses.Split(' '), CancellationToken);

        return await SendFeedback
        (
            coursesAssignmentResult,
            _feedbackService,
            "Successfully assigned these courses to you",
            true,
            CancellationToken
        );
    }

    /// <summary>
    /// Leave the given courses.
    /// </summary>
    /// <param name="courses">The courses to leave separated by space.</param>
    /// <returns>A result that may or may not be successful.</returns>
    [Command("leave")]
    [Description("Leave channels matching the given courses.")]
    [RequirePermission("courses.leave")]
    public async Task<IResult> HandleLeaveAsync([Description("Courses to leave separated by space")] string courses)
    {
        var discordUser = new DiscordUser(_commandContext.User.ID);

        var coursesAssignmentResultResult = await _channelAssigner.DeassignCourses
            (discordUser, courses.Split(' '), CancellationToken);
        if (!coursesAssignmentResultResult.IsDefined(out var coursesAssignmentResult))
        {
            return coursesAssignmentResultResult;
        }

        return await SendFeedback
        (
            coursesAssignmentResult,
            _feedbackService,
            "Successfully deassigned you from these courses",
            false,
            CancellationToken
        );
    }

    /// <summary>
    /// Toggle the given courses.
    /// </summary>
    /// <param name="courses">The courses to toggle separated by space.</param>
    /// <returns>A result that may or may not be successful.</returns>
    [Command("toggle")]
    [Description("Toggle (join if not present, leave if present) channels matching the given courses.")]
    [RequirePermission("courses.toggle")]
    public async Task<IResult> HandleToggleAsync([Description("Courses to toggle separated by space")] string courses)
    {
        var discordUser = new DiscordUser(_commandContext.User.ID);

        var coursesAssignmentResult = await _channelAssigner.ToggleCourses
            (discordUser, courses.Split(' '), CancellationToken);

        return await SendFeedback
        (
            coursesAssignmentResult,
            _feedbackService,
            "Successfully toggled these courses",
            false,
            CancellationToken
        );
    }

    private static async Task<IResult> SendFeedback
    (
        CoursesAssignmentResult coursesAssignmentResult,
        FeedbackService feedbackService,
        string successPrefix,
        bool featureMissing,
        CancellationToken ct
    )
    {
        var errors = coursesAssignmentResult.ErrorfulResults.Values.ToList();

        if (coursesAssignmentResult.SuccessCourses.Count > 0)
        {
            var feedbackResult = await feedbackService.SendContextualSuccessAsync
            (
                $"{successPrefix}: \n" + string.Join
                (
                    '\n',
                    coursesAssignmentResult.SuccessCourses.Select
                        (x => $"  **<#{x.ChannelId}>** - {x.CourseName} ({x.CourseKey})")
                ),
                ct: ct
            );

            if (!feedbackResult.IsSuccess)
            {
                errors.Add(Result.FromError(feedbackResult));
            }
        }

        if (featureMissing && coursesAssignmentResult.MissingCourses.Count > 0)
        {
            var feedbackResult = await feedbackService.SendContextualWarningAsync
            (
                "Could not find these courses, there aren't channels for them: " + string.Join
                    (", ", coursesAssignmentResult.MissingCourses)
                + ". If you want these courses to be added, contact administrators.",
                ct: ct
            );

            if (!feedbackResult.IsSuccess)
            {
                errors.Add(Result.FromError(feedbackResult));
            }
        }

        if (coursesAssignmentResult.ErrorfulResults.Count > 0)
        {
            var feedbackResult = await feedbackService.SendContextualErrorAsync
                ("There were some errors, contact administrators.", ct: ct);

            if (!feedbackResult.IsSuccess)
            {
                errors.Add(Result.FromError(feedbackResult));
            }
        }

        return errors.Count switch
        {
            0 => Result.FromSuccess(),
            1 => errors[0],
            _ => new AggregateError(errors.Cast<IResult>().ToArray())
        };
    }

    /// <summary>
    /// Semesters sub command group.
    /// </summary>
    [Group("semester")]
    [RequirePermission("courses.courses.semester")]
    public class SemesterCommands : CommandGroup
    {
        private readonly FeedbackService _feedbackService;
        private readonly ICommandContext _commandContext;
        private readonly CoursesInfo _coursesInfo;
        private readonly CoursesChannelAssigner _channelAssigner;
        private readonly IReadableDbContext<ChristofelBaseContext> _baseContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SemesterCommands"/> class.
        /// </summary>
        /// <param name="feedbackService">The feedback service.</param>
        /// <param name="commandContext">The command context.</param>
        /// <param name="coursesInfo">The courses info.</param>
        /// <param name="channelAssigner">The courses channel assigner.</param>
        /// <param name="baseContext">The readable christofel base database context.</param>
        public SemesterCommands
        (
            FeedbackService feedbackService,
            ICommandContext commandContext,
            CoursesInfo coursesInfo,
            CoursesChannelAssigner channelAssigner,
            IReadableDbContext<ChristofelBaseContext> baseContext
        )
        {
            _feedbackService = feedbackService;
            _commandContext = commandContext;
            _coursesInfo = coursesInfo;
            _channelAssigner = channelAssigner;
            _baseContext = baseContext;
        }

        /// <summary>
        /// Join all courses of the given semester.
        /// </summary>
        /// <param name="semester">The semester selector.</param>
        /// <returns>A result that may or may not have succeeded.</returns>
        [Command("joinall")]
        [Description("Join all courses you are enrolled to in the given semester.")]
        [RequirePermission("courses.courses.semester.joinall")]
        public async Task<IResult> HandleJoinAllAsync(SemesterSelector semester)
        {
            var dbUser = await _baseContext.Set<DbUser>()
                .Authenticated()
                .Where(x => x.DiscordId == _commandContext.User.ID)
                .FirstOrDefaultAsync(CancellationToken);

            if (dbUser is null)
            {
                await _feedbackService.SendContextualErrorAsync
                    ("You are not authenticated, cannot assign courses to you.", ct: CancellationToken);
                return Result.FromError
                    (new InvalidOperationError("User not authenticated, but tried to assign semester courses."));
            }

            var coursesAssignmentResultResult = await _channelAssigner.AssignSemesterCourses
                (new LinkUser(dbUser), semester.ToString().ToLower(), CancellationToken);

            if (!coursesAssignmentResultResult.IsDefined(out var coursesAssignmentResult))
            {
                await _feedbackService.SendContextualErrorAsync("There was an error, contact administrators.");
                return coursesAssignmentResultResult;
            }

            return await SendFeedback
            (
                coursesAssignmentResult,
                _feedbackService,
                "Successfully assigned these courses to you",
                true,
                CancellationToken
            );
        }

        /// <summary>
        /// Join all courses of the given semester.
        /// </summary>
        /// <param name="semester">The semester selector.</param>
        /// <returns>A result that may or may not have succeeded.</returns>
        [Command("leaveall")]
        [Description("Leave all courses you are enrolled to in the given semester.")]
        [RequirePermission("courses.courses.semester.leaveall")]
        public async Task<IResult> HandleLeaveAllAsync(SemesterSelector semester)
        {
            var dbUser = await _baseContext.Set<DbUser>()
                .Authenticated()
                .Where(x => x.DiscordId == _commandContext.User.ID)
                .FirstOrDefaultAsync(CancellationToken);

            if (dbUser is null)
            {
                await _feedbackService.SendContextualErrorAsync
                    ("You are not authenticated, cannot assign courses to you.", ct: CancellationToken);
                return Result.FromError
                    (new InvalidOperationError("User not authenticated, but tried to assign semester courses."));
            }

            var coursesAssignmentResultResult = await _channelAssigner.DeassignSemesterCourses
                (new LinkUser(dbUser), semester.ToString().ToLower(), CancellationToken);

            if (!coursesAssignmentResultResult.IsDefined(out var coursesAssignmentResult))
            {
                await _feedbackService.SendContextualErrorAsync("There was an error, contact administrators.");
                return coursesAssignmentResultResult;
            }

            return await SendFeedback
            (
                coursesAssignmentResult,
                _feedbackService,
                "Successfully deassigned you from these courses",
                true,
                CancellationToken
            );
        }

        /// <summary>
        /// Show all your cuorses in the given semester.
        /// </summary>
        /// <param name="selector">The semester selector to show courses for.</param>
        /// <returns>A result that may or may not have succeeded.</returns>
        [Command("show")]
        [Description("Show all your courses in the given semester.")]
        [RequirePermission("courses.courses.semester.show")]
        public async Task<IResult> HandleShowAsync(SemesterSelector selector)
        {
            var dbUser = await _baseContext.Set<DbUser>()
                .Authenticated()
                .Where(x => x.DiscordId == _commandContext.User.ID)
                .FirstOrDefaultAsync(CancellationToken);

            if (dbUser is null)
            {
                await _feedbackService.SendContextualErrorAsync
                    ("You are not authenticated, cannot assign courses to you.", ct: CancellationToken);
                return Result.FromError
                    (new InvalidOperationError("User not authenticated, but tried to assign semester courses."));
            }

            var coursesResult = await _coursesInfo.GetSemesterCourses
                (new LinkUser(dbUser), selector.ToString().ToLower(), CancellationToken);
            if (!coursesResult.IsDefined(out var courses))
            {
                await _feedbackService.SendContextualErrorAsync("There was an error, contact administrators.");
                return coursesResult;
            }

            if (courses.Count == 0)
            {
                return await _feedbackService.SendContextualInfoAsync
                    ("Could not find any courses you are enrolled in for the given semester.");
            }

            return await _feedbackService.SendContextualSuccessAsync
            (
                "These are the courses you are enrolled in and are available on the server:\n" + string.Join
                (
                    '\n',
                    courses.Select(x => $"  **<#{x.ChannelId}>** - {x.CourseName} ({x.CourseKey})")
                )
            );
        }
    }
}