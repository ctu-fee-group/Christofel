//
//   CoursesCommands.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
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
    private readonly CoursesChannelUserAssigner _channelUserAssigner;
    private readonly CoursesRepository _coursesRepository;
    private readonly IReadableDbContext<ChristofelBaseContext> _baseContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoursesCommands"/> class.
    /// </summary>
    /// <param name="feedbackService">The feedback service.</param>
    /// <param name="commandContext">The command context.</param>
    /// <param name="channelUserAssigner">The courses channel assigner.</param>
    /// <param name="coursesRepository">The courses respository.</param>
    /// <param name="baseContext">The readable christofel base database context.</param>
    public CoursesCommands
    (
        FeedbackService feedbackService,
        ICommandContext commandContext,
        CoursesChannelUserAssigner channelUserAssigner,
        CoursesRepository coursesRepository,
        IReadableDbContext<ChristofelBaseContext> baseContext
    )
    {
        _feedbackService = feedbackService;
        _commandContext = commandContext;
        _channelUserAssigner = channelUserAssigner;
        _coursesRepository = coursesRepository;
        _baseContext = baseContext;
    }

    /// <summary>
    /// Join the given courses.
    /// </summary>
    /// <param name="courses">The courses to join separated by space.</param>
    /// <returns>A result that may or may not be successful.</returns>
    [Command("join")]
    [Description("Join channels matching the given courses.")]
    [RequirePermission("courses.courses.join")]
    public async Task<IResult> JoinAsync([Description("Courses to join separated by space")] string courses)
    {
        var discordUser = new DiscordUser(_commandContext.User.ID);

        var coursesAssignmentResult = await _channelUserAssigner.AssignCourses
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
    [RequirePermission("courses.courses.leave")]
    public async Task<IResult> HandleLeaveAsync([Description("Courses to leave separated by space")] string courses)
    {
        var discordUser = new DiscordUser(_commandContext.User.ID);

        var coursesAssignmentResultResult = await _channelUserAssigner.DeassignCourses
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
    [RequirePermission("courses.courses.toggle")]
    public async Task<IResult> HandleToggleAsync([Description("Courses to toggle separated by space")] string courses)
    {
        var discordUser = new DiscordUser(_commandContext.User.ID);

        var coursesAssignmentResult = await _channelUserAssigner.ToggleCourses
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

    /// <summary>
    /// Search the given courses.
    /// </summary>
    /// <param name="courses">The courses to toggle separated by space.</param>
    /// <returns>A result that may or may not be successful.</returns>
    [Command("search")]
    [Description("Fuzzy search given courses.")]
    [RequirePermission("courses.courses.search")]
    public async Task<IResult> HandleSearchAsync
        ([Description("Parts of names or keys of the courses to search for separated by space.")] string courses)
    {
        var coursesAssignmentResult = await _coursesRepository
            .SearchCourseAssignments
            (
                CancellationToken,
                courses
                    .Split(' ', ',', StringSplitOptions.TrimEntries)
            );

        if (!coursesAssignmentResult.IsDefined(out var coursesAssignments))
        {
            await _feedbackService.SendContextualErrorAsync("There was an error, contact administrators.");
            return coursesAssignmentResult;
        }

        if (coursesAssignments.Count == 0)
        {
            return await _feedbackService.SendContextualInfoAsync("Could not find any courses with the given criteria.");
        }

        return await _feedbackService.SendContextualSuccessAsync
            ("Found these courses: \n" + CoursesFormatter.FormatCoursesMessage(coursesAssignments));
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

        if (coursesAssignmentResult.MissingCourses.Count == 0 && coursesAssignmentResult.ErrorfulResults.Count == 0
            && coursesAssignmentResult.SuccessCourses.Count == 0)
        {
            await feedbackService.SendContextualWarningAsync("Could not find any courses in the given period.", ct: ct);
        }

        if (coursesAssignmentResult.SuccessCourses.Count > 0)
        {
            var feedbackResult = await feedbackService.SendContextualSuccessAsync
            (
                $"{successPrefix}: \n" + CoursesFormatter.FormatCoursesMessage(coursesAssignmentResult.SuccessCourses),
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
        private readonly CoursesRepository _coursesRepository;
        private readonly CoursesChannelUserAssigner _channelUserAssigner;
        private readonly CurrentSemesterCache _currentSemesterCache;
        private readonly IReadableDbContext<ChristofelBaseContext> _baseContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SemesterCommands"/> class.
        /// </summary>
        /// <param name="feedbackService">The feedback service.</param>
        /// <param name="commandContext">The command context.</param>
        /// <param name="coursesRepository">The courses info.</param>
        /// <param name="channelUserAssigner">The courses channel assigner.</param>
        /// <param name="currentSemesterCache">The current semester cache.</param>
        /// <param name="baseContext">The readable christofel base database context.</param>
        public SemesterCommands
        (
            FeedbackService feedbackService,
            ICommandContext commandContext,
            CoursesRepository coursesRepository,
            CoursesChannelUserAssigner channelUserAssigner,
            CurrentSemesterCache currentSemesterCache,
            IReadableDbContext<ChristofelBaseContext> baseContext
        )
        {
            _feedbackService = feedbackService;
            _commandContext = commandContext;
            _coursesRepository = coursesRepository;
            _channelUserAssigner = channelUserAssigner;
            _currentSemesterCache = currentSemesterCache;
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

            var coursesAssignmentResultResult = await _channelUserAssigner.AssignSemesterCourses
                (new LinkUser(dbUser), await GetSemester(semester), CancellationToken);

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

            var coursesAssignmentResultResult = await _channelUserAssigner.DeassignSemesterCourses
                (new LinkUser(dbUser), await GetSemester(semester), CancellationToken);

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
        /// <param name="semester">The semester selector to show courses for.</param>
        /// <returns>A result that may or may not have succeeded.</returns>
        [Command("show")]
        [Description("Show all your courses in the given semester.")]
        [RequirePermission("courses.courses.semester.show")]
        public async Task<IResult> HandleShowAsync(SemesterSelector semester)
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

            var coursesResult = await _coursesRepository.GetSemesterCourses
                (new LinkUser(dbUser), await GetSemester(semester), CancellationToken);
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

        private async Task<string> GetSemester(SemesterSelector semesterSelector)
        {
            var currentSemester = await _currentSemesterCache.GetCurrentSemester();

            return semesterSelector switch
            {
                SemesterSelector.Previous => currentSemester.GetPreviousSemester().ToString(),
                SemesterSelector.Current => currentSemester.ToString(),
                SemesterSelector.Next => currentSemester.GetNextSemester().ToString(),
                _ => throw new Exception()
            };
        }
    }
}