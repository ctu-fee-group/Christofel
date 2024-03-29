//
//  CoursesInteractionsResponder.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.BaseLib.Extensions;
using Christofel.CommandsLib.Attributes;
using Christofel.Common.Database;
using Christofel.Common.Database.Models;
using Christofel.Courses.Data;
using Christofel.Courses.Extensions;
using Christofel.CoursesLib.Services;
using Christofel.Helpers.Localization;
using Microsoft.EntityFrameworkCore;
using OneOf;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Feedback.Messages;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Interactivity;
using Remora.Rest.Core;
using Remora.Results;
using CustomIDHelpers = Christofel.LGPLicensed.Interactivity.CustomIDHelpers;

namespace Christofel.Courses.Interactivity.Commands;

/// <summary>
/// Handles courses interactivity commands.
/// </summary>
[Group("coursesint")]
[Ephemeral]
public class CoursesInteractionsResponder : CommandGroup
{
    private readonly IInteractionContext _commandContext;
    private readonly FeedbackService _feedbackService;
    private readonly CoursesRepository _coursesRepository;
    private readonly CoursesChannelUserAssigner _channelUserAssigner;
    private readonly CoursesInteractivityFormatter _coursesInteractivityFormatter;
    private readonly CourseMessageInteractivity _courseMessageInteractivity;
    private readonly LocalizedStringLocalizer<CoursesPlugin> _localizer;
    private readonly ChristofelBaseContext _baseContext;
    private readonly CurrentSemesterCache _currentSemesterCache;
    private readonly InteractivityCultureProvider _cultureProvider;
    private readonly FeedbackData _feedbackData;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoursesInteractionsResponder"/> class.
    /// </summary>
    /// <param name="commandContext">The command context.</param>
    /// <param name="interactionApi">The interaction api.</param>
    /// <param name="feedbackService">The feedback service.</param>
    /// <param name="coursesRepository">The courses repository.</param>
    /// <param name="channelUserAssigner">The courses channel user assigner.</param>
    /// <param name="coursesInteractivityFormatter">The courses interactivity formatter.</param>
    /// <param name="courseMessageInteractivity">The course message interactivity.</param>
    /// <param name="localizer">The localizer.</param>
    /// <param name="baseContext">The christofel base context.</param>
    /// <param name="currentSemesterCache">The current semester cache.</param>
    /// <param name="cultureProvider">The culture provider.</param>
    public CoursesInteractionsResponder
    (
        IInteractionContext commandContext,
        IDiscordRestInteractionAPI interactionApi,
        FeedbackService feedbackService,
        CoursesRepository coursesRepository,
        CoursesChannelUserAssigner channelUserAssigner,
        CoursesInteractivityFormatter coursesInteractivityFormatter,
        CourseMessageInteractivity courseMessageInteractivity,
        LocalizedStringLocalizer<CoursesPlugin> localizer,
        ChristofelBaseContext baseContext,
        CurrentSemesterCache currentSemesterCache,
        InteractivityCultureProvider cultureProvider
    )
    {
        _commandContext = commandContext;
        _feedbackService = feedbackService;
        _coursesRepository = coursesRepository;
        _channelUserAssigner = channelUserAssigner;
        _coursesInteractivityFormatter = coursesInteractivityFormatter;
        _courseMessageInteractivity = courseMessageInteractivity;
        _localizer = localizer;
        _baseContext = baseContext;
        _currentSemesterCache = currentSemesterCache;
        _cultureProvider = cultureProvider;
        _feedbackData = new FeedbackData(commandContext, interactionApi, feedbackService);
    }

    /// <summary>
    /// Join given semester courses.
    /// </summary>
    /// <param name="language">The language of the response.</param>
    /// <param name="commandType">The type of the command to execute.</param>
    /// <param name="proceedImmediately">Whether to join the enrolled courses now (true) or only show the courses.</param>
    /// <param name="semesterSelector">The semester selector.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    [Button("semester")]
    [InteractionCallbackType(InteractionCallbackType.DeferredUpdateMessage)]
    public async Task<IResult> HandleSemesterButtonAsync
    (
        string language,
        InteractivityCommandType commandType,
        bool proceedImmediately,
        SemesterSelector semesterSelector
    )
    {
        if (!_commandContext.TryGetUserID(out var userId))
        {
            return (Result)new GenericError("Could not get user id from context.");
        }

        _cultureProvider.CurrentCulture = language;
        var dbUser = await _baseContext.Set<DbUser>()
            .Authenticated()
            .Where(x => x.DiscordId == userId)
            .FirstOrDefaultAsync(CancellationToken);

        if (dbUser is null)
        {
            await _feedbackService.SendContextualErrorAsync
            (
                _localizer.Translate("NOT_AUTHENTICATED", language),
                options: new FeedbackMessageOptions(MessageFlags: MessageFlags.Ephemeral),
                ct: CancellationToken
            );
            return Result.FromError
                (new InvalidOperationError("User not authenticated, but tried to assign semester courses."));
        }

        var coursesResult = await _coursesRepository.GetSemesterCoursesKeys
            (new LinkUser(dbUser), await GetSemester(semesterSelector), CancellationToken);
        if (!coursesResult.IsDefined(out var courses))
        {
            await _feedbackService.SendContextualErrorAsync
            (
                _localizer.Translate("ERROR", language),
                options: new FeedbackMessageOptions(MessageFlags: MessageFlags.Ephemeral),
                ct: CancellationToken
            );
            return coursesResult;
        }

        if (courses.Count == 0)
        {
            return await _feedbackService.SendContextualInfoAsync
            (
                _localizer.Translate("COURSE_BY_SEMESTER_NOT_FOUND", language),
                options: new FeedbackMessageOptions(MessageFlags: MessageFlags.Ephemeral),
                ct: CancellationToken
            );
        }

        if (proceedImmediately)
        {
            return await _courseMessageInteractivity.HandleCourseCommand
                (commandType, courses, CancellationToken);
        }

        var courseAssignmentsResult = await _coursesRepository.GetCourseAssignments
            (CancellationToken, courses.ToArray());
        if (!courseAssignmentsResult.IsDefined(out var courseAssignments))
        {
            return courseAssignmentsResult;
        }

        if (courseAssignments.Count == 0)
        {
            return await _feedbackService.SendContextualInfoAsync
            (
                _localizer.Translate("COURSES_MISSING", language, string.Join(", ", courses)),
                options: new FeedbackMessageOptions(MessageFlags: MessageFlags.Ephemeral),
                ct: CancellationToken
            );
        }

        return await _courseMessageInteractivity.SendCoursesMessagesAsync
            (_localizer.Translate("COURSE_BY_SEMESTER_CHOOSE"), courseAssignments, CancellationToken);
    }

    /// <summary>
    /// Send courses based on given department.
    /// </summary>
    /// <param name="language">The language of the response.</param>
    /// <param name="department">The key of the department.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    [Button("department")]
    [InteractionCallbackType(InteractionCallbackType.DeferredUpdateMessage)]
    public async Task<IResult> HandleDepartmentButtonAsync
        (string language, string department)
    {
        _cultureProvider.CurrentCulture = language;
        var courseAssignmentsResult = await _coursesRepository.GetCoursesByDepartment(department, CancellationToken);
        if (!courseAssignmentsResult.IsDefined(out var courseAssignments))
        {
            await _feedbackService.SendContextualErrorAsync(_localizer.Translate("ERROR", language));
            return courseAssignmentsResult;
        }

        return await _courseMessageInteractivity.SendCoursesMessagesAsync
            (_localizer.Translate("DEPARTMENTS_COURSES"), courseAssignments, CancellationToken);
    }

    /// <summary>
    /// Execute the given command (join/leave/toggle the given course) by channel id.
    /// </summary>
    /// <param name="language">The language of the response.</param>
    /// <param name="channelId">The channel id of the course.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    [Button("course")]
    [InteractionCallbackType(InteractionCallbackType.DeferredUpdateMessage)]
    public async Task<IResult> HandleCourseAsync
        (string language, Snowflake channelId)
    {
        _cultureProvider.CurrentCulture = language;
        var coursesByChannelResult = await _coursesRepository.GetCoursesByChannel(channelId, CancellationToken);

        if (!coursesByChannelResult.IsDefined(out var coursesByChannel))
        {
            return coursesByChannelResult;
        }

        if (coursesByChannel.Count == 0)
        {
            await _feedbackService.SendContextualWarningAsync
            (
                _localizer.Translate("COURSE_BY_CHANNEL_NOT_FOUND", language, channelId.ToString()),
                ct: CancellationToken
            );
            return (Result)new InvalidOperationError($"Could not find any courses for channel <#{channelId}>");
        }

        // TODO: make methods accepting CourseAssignment to avoid calling the database twice.
        var courseKeys = coursesByChannel.Select(x => x.CourseKey);

        return await _courseMessageInteractivity.HandleCourseCommand
            (InteractivityCommandType.Toggle, courseKeys, CancellationToken);
    }

    /// <summary>
    /// Execute the given command (join/leave/toggle the given course) by course keys separated by space.
    /// </summary>
    /// <param name="language">The language of the response.</param>
    /// <param name="commandType">The type of the command to execute.</param>
    /// <param name="courses">The keys of the courses to join.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    [Modal("courses")]
    public async Task<IResult> HandleCourseAsync
        (string language, InteractivityCommandType commandType, [Greedy] string courses)
    {
        _cultureProvider.CurrentCulture = language;
        return await _courseMessageInteractivity.HandleCourseCommand
            (commandType, courses.Split(' '), CancellationToken);
    }

    /// <summary>
    /// Search for courses separated by space.
    /// </summary>
    /// <param name="language">The language of the response.</param>
    /// <param name="courses">The keys/names/ of the courses to search for.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    [Modal("search")]
    public async Task<IResult> HandleSearchAsync
        (string language, [Greedy] string courses)
    {
        if (!_commandContext.TryGetUserID(out var userId))
        {
            return (Result)new GenericError("Could not get user id from context.");
        }

        _cultureProvider.CurrentCulture = language;
        var coursesAssignmentResult = await _coursesRepository
            .SearchCourseAssignments
            (
                CancellationToken,
                courses
                    .Split(' ', ',', StringSplitOptions.TrimEntries)
            );

        if (!coursesAssignmentResult.IsDefined(out var courseAssignments))
        {
            await _feedbackService.SendContextualErrorAsync
            (
                _localizer.Translate("ERROR", language),
                options: new FeedbackMessageOptions(MessageFlags: MessageFlags.Ephemeral),
                ct: CancellationToken
            );
            return coursesAssignmentResult;
        }

        if (courseAssignments.Count == 0)
        {
            await _feedbackService.SendContextualInfoAsync
            (
                _localizer.Translate("COURSES_NOT_FOUND", language),
                options: new FeedbackMessageOptions(MessageFlags: MessageFlags.Ephemeral),
                ct: CancellationToken
            );
        }

        var joinedCoursesResult = await _coursesRepository.JoinWithUserData
            (courseAssignments, userId, CancellationToken);

        if (!joinedCoursesResult.IsDefined(out var joinedCourses))
        {
            return joinedCoursesResult;
        }

        return await _courseMessageInteractivity.SendCoursesMessagesAsync
            (_localizer.Translate("SEARCH_COURSE_SUCCESS"), courseAssignments, CancellationToken);
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

    /// <summary>
    /// Subgroup for buttons on the main message.
    /// </summary>
    [Group("main")]
    [Ephemeral]
    public class MainMessage : CommandGroup
    {
        private readonly IInteractionContext _interactionContext;
        private readonly IDiscordRestInteractionAPI _interactionApi;
        private readonly CoursesInteractivityFormatter _coursesInteractivityFormatter;
        private readonly CoursesRepository _coursesRepository;
        private readonly FeedbackService _feedbackService;
        private readonly IStringLocalizer<CoursesPlugin> _localizer;
        private readonly InteractivityCultureProvider _cultureProvider;
        private readonly FeedbackData _feedbackData;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainMessage"/> class.
        /// </summary>
        /// <param name="interactionContext">The interaction context.</param>
        /// <param name="interactionApi">The interaction api.</param>
        /// <param name="coursesInteractivityFormatter">The courses interactivity formatter.</param>
        /// <param name="coursesRepository">The courses repository.</param>
        /// <param name="feedbackService">The feedback service.</param>
        /// <param name="localizer">The string localizer.</param>
        /// <param name="cultureProvider">The culture provider.</param>
        public MainMessage
        (
            IInteractionContext interactionContext,
            IDiscordRestInteractionAPI interactionApi,
            CoursesInteractivityFormatter coursesInteractivityFormatter,
            CoursesRepository coursesRepository,
            FeedbackService feedbackService,
            IStringLocalizer<CoursesPlugin> localizer,
            InteractivityCultureProvider cultureProvider
        )
        {
            _interactionContext = interactionContext;
            _interactionApi = interactionApi;
            _coursesInteractivityFormatter = coursesInteractivityFormatter;
            _coursesRepository = coursesRepository;
            _feedbackService = feedbackService;
            _localizer = localizer;
            _cultureProvider = cultureProvider;
            _feedbackData = new FeedbackData(interactionContext, interactionApi, feedbackService);
        }

        /// <summary>
        /// Open a modal with a textbox to add given courses.
        /// </summary>
        /// <param name="language">The language of the modal.</param>
        /// <param name="commandType">The type of the command executed.</param>
        /// <returns>A result that may or may not have succeeded.</returns>
        [Button("keys")]
        [SuppressInteractionResponse(true)]
        public async Task<Result> HandleKeysButtonAsync(string language, InteractivityCommandType commandType)
        {
            _cultureProvider.CurrentCulture = language;
            return await _interactionApi.CreateInteractionResponseAsync
            (
                _interactionContext.Interaction.ID,
                _interactionContext.Interaction.Token,
                new InteractionResponse
                (
                    InteractionCallbackType.Modal,
                    new Optional<OneOf<IInteractionMessageCallbackData, IInteractionAutocompleteCallbackData,
                        IInteractionModalCallbackData>>
                    (
                        new InteractionModalCallbackData
                        (
                            CustomIDHelpers.CreateModalID("courses", "coursesint", language, commandType.ToString()),
                            _localizer.Translate($"COURSE_MODAL_TITLE_{commandType}", language),
                            new[]
                            {
                                new ActionRowComponent
                                (
                                    new[]
                                    {
                                        new TextInputComponent
                                        (
                                            "courses",
                                            TextInputStyle.Short,
                                            _localizer.Translate($"COURSE_MODAL_TEXTINPUT_{commandType}", language),
                                            1,
                                            4000,
                                            true,
                                            default,
                                            _localizer.Translate($"COURSE_MODAL_PLACEHOLDER_{commandType}", language)
                                        )
                                    }
                                )

                            }
                        )
                    )
                ),
                ct: CancellationToken
            );
        }

        /// <summary>
        /// Open a modal with a search textbox to search for courses.
        /// </summary>
        /// <param name="language">The language of the modal.</param>
        /// <returns>A result that may or may not have succeeded.</returns>
        [Button("search")]
        [SuppressInteractionResponse(true)]
        public async Task<Result> HandleSearchButtonAsync(string language)
        {
            _cultureProvider.CurrentCulture = language;
            return await _interactionApi.CreateInteractionResponseAsync
            (
                _interactionContext.Interaction.ID,
                _interactionContext.Interaction.Token,
                new InteractionResponse
                (
                    InteractionCallbackType.Modal,
                    new Optional<OneOf<IInteractionMessageCallbackData, IInteractionAutocompleteCallbackData,
                        IInteractionModalCallbackData>>
                    (
                        new InteractionModalCallbackData
                        (
                            CustomIDHelpers.CreateModalID("search", "coursesint", language),
                            _localizer.Translate("SEARCH_MODAL_TITLE", language),
                            new[]
                            {
                                new ActionRowComponent
                                (
                                    new[]
                                    {
                                        new TextInputComponent
                                        (
                                            "courses",
                                            TextInputStyle.Short,
                                            _localizer.Translate("SEARCH_MODAL_TEXTINPUT", language),
                                            1,
                                            4000,
                                            true,
                                            default,
                                            _localizer.Translate("SEARCH_MODAL_PLACEHOLDER", language)
                                        )
                                    }
                                )

                            }
                        )
                    )
                ),
                ct: CancellationToken
            );
        }

        /// <summary>
        /// Send message with semester selectors.
        /// </summary>
        /// <param name="language">The language of the message.</param>
        /// <param name="proceedImmediately">Whether to join the enrolled courses now (true) or only show the courses.</param>
        /// <param name="commandType">The type of the command executed.</param>
        /// <returns>A result that may or may not have succeeded.</returns>
        [Button("semesters")]
        public async Task<IResult> HandleSemestersButtonAsync
            (string language, bool proceedImmediately, InteractivityCommandType commandType)
        {
            _cultureProvider.CurrentCulture = language;
            var semesterMessage = _coursesInteractivityFormatter.FormatSemesterMessage
                (string.Empty, language, commandType, proceedImmediately);

            return await _feedbackData.SendContextualMessageDataAsync
                (new[] { semesterMessage }, false, CancellationToken);
        }

        /// <summary>
        /// Send message with all departments.
        /// </summary>
        /// <param name="language">The language of the message.</param>
        /// <returns>A result that may or may not have succeeded.</returns>
        [Button("departments")]
        public async Task<IResult> HandleDepartmentsButtonAsync(string language)
        {
            _cultureProvider.CurrentCulture = language;
            var departmentsResult = await _coursesRepository.GetDepartments(false, CancellationToken);
            if (!departmentsResult.IsDefined(out var departments))
            {
                return departmentsResult;
            }

            var departmentMessages = _coursesInteractivityFormatter.FormatDepartmentsMessage
                (string.Empty, departments);

            return await _feedbackData.SendContextualMessageDataAsync(departmentMessages, false, CancellationToken);
        }

        /// <summary>
        /// Send the main message translated in the given language.
        /// </summary>
        /// <param name="language">The language of the message.</param>
        /// <returns>A result that may or may not have succeeded.</returns>
        [Button("translate")]
        public async Task<IResult> HandleTranslateButtonAsync(string language)
        {
            _cultureProvider.CurrentCulture = language;
            var mainMessage = _coursesInteractivityFormatter.FormatMainMessage(string.Empty);
            return await _feedbackData.SendContextualMessageDataAsync(new[] { mainMessage }, false, CancellationToken);
        }
    }
}