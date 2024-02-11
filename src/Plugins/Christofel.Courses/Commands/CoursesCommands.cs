//
//   CoursesCommands.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Globalization;
using Christofel.BaseLib.Extensions;
using Christofel.CommandsLib.Permissions;
using Christofel.Common.Database;
using Christofel.Common.Database.Models;
using Christofel.Courses.Data;
using Christofel.Courses.Extensions;
using Christofel.Courses.Interactivity;
using Christofel.CoursesLib.Services;
using Christofel.Helpers.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
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
    private readonly CoursesInteractivityFormatter _coursesInteractivityFormatter;
    private readonly CourseMessageInteractivity _courseMessageInteractivity;
    private readonly IReadableDbContext<ChristofelBaseContext> _baseContext;
    private readonly LocalizedStringLocalizer<CoursesPlugin> _localizer;
    private readonly CoursesAssignmentOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoursesCommands"/> class.
    /// </summary>
    /// <param name="feedbackService">The feedback service.</param>
    /// <param name="commandContext">The command context.</param>
    /// <param name="channelUserAssigner">The courses channel assigner.</param>
    /// <param name="coursesRepository">The courses respository.</param>
    /// <param name="coursesInteractivityFormatter">The courses interactivity formatter.</param>
    /// <param name="courseMessageInteractivity">The course message interactivity.</param>
    /// <param name="baseContext">The readable christofel base database context.</param>
    /// <param name="localizer">The string localizer.</param>
    /// <param name="cultureProvider">The culture provider.</param>
    /// <param name="options">The assignment options.</param>
    public CoursesCommands
    (
        FeedbackService feedbackService,
        ICommandContext commandContext,
        CoursesChannelUserAssigner channelUserAssigner,
        CoursesRepository coursesRepository,
        CoursesInteractivityFormatter coursesInteractivityFormatter,
        CourseMessageInteractivity courseMessageInteractivity,
        IReadableDbContext<ChristofelBaseContext> baseContext,
        LocalizedStringLocalizer<CoursesPlugin> localizer,
        InteractivityCultureProvider cultureProvider,
        IOptionsSnapshot<CoursesAssignmentOptions> options
    )
    {
        _feedbackService = feedbackService;
        _commandContext = commandContext;
        _channelUserAssigner = channelUserAssigner;
        _coursesRepository = coursesRepository;
        _coursesInteractivityFormatter = coursesInteractivityFormatter;
        _courseMessageInteractivity = courseMessageInteractivity;
        _baseContext = baseContext;
        _localizer = localizer;
        _options = options.Value;

        cultureProvider.CurrentCulture = "en";
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
        if (!_commandContext.TryGetUserID(out var userId))
        {
            return (Result)new GenericError("Could not get user id from context.");
        }

        var discordUser = new DiscordUser(userId);

        var coursesAssignmentResult = await _channelUserAssigner.AssignCourses
            (discordUser, courses.Split(' '), CancellationToken);

        return await CourseMessageInteractivity.SendFeedback
        (
            _localizer,
            coursesAssignmentResult,
            _feedbackService,
            _options,
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
        if (!_commandContext.TryGetUserID(out var userId))
        {
            return (Result)new GenericError("Could not get user id from context.");
        }

        var discordUser = new DiscordUser(userId);

        var coursesAssignmentResult = await _channelUserAssigner.DeassignCourses
            (discordUser, courses.Split(' '), CancellationToken);

        return await CourseMessageInteractivity.SendFeedback
        (
            _localizer,
            coursesAssignmentResult,
            _feedbackService,
            _options,
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
        if (!_commandContext.TryGetUserID(out var userId))
        {
            return (Result)new GenericError("Could not get user id from context.");
        }

        var discordUser = new DiscordUser(userId);

        var coursesAssignmentResult = await _channelUserAssigner.ToggleCourses
            (discordUser, courses.Split(' '), CancellationToken);

        return await CourseMessageInteractivity.SendFeedback
        (
            _localizer,
            coursesAssignmentResult,
            _feedbackService,
            _options,
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
            return await _feedbackService.SendContextualInfoAsync
                ("Could not find any courses with the given criteria.");
        }

        return await _courseMessageInteractivity.SendCoursesMessagesAsync
            ("Found these courses.", coursesAssignments, CancellationToken);
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
        private readonly CoursesInteractivityFormatter _coursesInteractivityFormatter;
        private readonly IReadableDbContext<ChristofelBaseContext> _baseContext;
        private readonly LocalizedStringLocalizer<CoursesPlugin> _localizer;
        private readonly CoursesAssignmentOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="SemesterCommands"/> class.
        /// </summary>
        /// <param name="feedbackService">The feedback service.</param>
        /// <param name="commandContext">The command context.</param>
        /// <param name="coursesRepository">The courses info.</param>
        /// <param name="channelUserAssigner">The courses channel assigner.</param>
        /// <param name="currentSemesterCache">The current semester cache.</param>
        /// <param name="coursesInteractivityFormatter">The courses interactivity formatter.</param>
        /// <param name="baseContext">The readable christofel base database context.</param>
        /// <param name="localizer">The string localizer.</param>
        /// <param name="options">The optcourse assignment options.</param>
        public SemesterCommands
        (
            FeedbackService feedbackService,
            ICommandContext commandContext,
            CoursesRepository coursesRepository,
            CoursesChannelUserAssigner channelUserAssigner,
            CurrentSemesterCache currentSemesterCache,
            CoursesInteractivityFormatter coursesInteractivityFormatter,
            IReadableDbContext<ChristofelBaseContext> baseContext,
            LocalizedStringLocalizer<CoursesPlugin> localizer,
            IOptionsSnapshot<CoursesAssignmentOptions> options
        )
        {
            _feedbackService = feedbackService;
            _commandContext = commandContext;
            _coursesRepository = coursesRepository;
            _channelUserAssigner = channelUserAssigner;
            _currentSemesterCache = currentSemesterCache;
            _coursesInteractivityFormatter = coursesInteractivityFormatter;
            _baseContext = baseContext;
            _localizer = localizer;
            _options = options.Value;
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
            if (!_commandContext.TryGetUserID(out var userId))
            {
                return (Result)new GenericError("Could not get user id from context.");
            }

            var dbUser = await _baseContext.Set<DbUser>()
                .Authenticated()
                .Where(x => x.DiscordId == userId)
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

            return await CourseMessageInteractivity.SendFeedback
            (
                _localizer,
                coursesAssignmentResult,
                _feedbackService,
                _options,
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
            if (!_commandContext.TryGetUserID(out var userId))
            {
                return (Result)new GenericError("Could not get user id from context.");
            }

            var dbUser = await _baseContext.Set<DbUser>()
                .Authenticated()
                .Where(x => x.DiscordId == userId)
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

            return await CourseMessageInteractivity.SendFeedback
            (
                _localizer,
                coursesAssignmentResult,
                _feedbackService,
                _options,
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
            if (!_commandContext.TryGetUserID(out var userId))
            {
                return (Result)new GenericError("Could not get user id from context.");
            }

            var dbUser = await _baseContext.Set<DbUser>()
                .Authenticated()
                .Where(x => x.DiscordId == userId)
                .FirstOrDefaultAsync(CancellationToken);

            if (dbUser is null)
            {
                await _feedbackService.SendContextualErrorAsync
                    ("You are not authenticated, cannot assign courses to you.", ct: CancellationToken);
                return Result.FromError
                    (new InvalidOperationError("User not authenticated, but tried to assign semester courses."));
            }

            var courseAssignmentsResult = await _coursesRepository.GetSemesterCourses
                (new LinkUser(dbUser), await GetSemester(semester), CancellationToken);
            if (!courseAssignmentsResult.IsDefined(out var courseAssignments))
            {
                await _feedbackService.SendContextualErrorAsync("There was an error, contact administrators.");
                return courseAssignmentsResult;
            }

            if (courseAssignments.Count == 0)
            {
                return await _feedbackService.SendContextualInfoAsync
                    ("Could not find any courses you are enrolled in for the given semester.");
            }

            var joinedCoursesResult = await _coursesRepository.JoinWithUserData
                (courseAssignments, userId, CancellationToken);

            if (!joinedCoursesResult.IsDefined(out var joinedCourses))
            {
                return joinedCoursesResult;
            }

            return await _feedbackService.SendContextualMessageDataAsync
            (
                _coursesInteractivityFormatter.FormatCoursesMessage
                (
                    "Found these courses you are enrolled in and are added on the server.",
                    joinedCourses
                ),
                CancellationToken
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