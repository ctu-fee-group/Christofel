//
//  CourseMessageInteractivity.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Courses.Data;
using Christofel.Courses.Extensions;
using Christofel.Courses.Jobs;
using Christofel.CoursesLib.Data;
using Christofel.CoursesLib.Database;
using Christofel.CoursesLib.Services;
using Christofel.Helpers.JobQueue;
using Christofel.Helpers.Localization;
using Christofel.Plugins.Lifetime;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Feedback.Messages;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Interactivity.Services;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.Courses.Interactivity;

/// <summary>
/// Handling sending course messages, assigning, deassigning course channels.
/// </summary>
public class CourseMessageInteractivity
{
    private readonly IInteractionContext _commandContext;
    private readonly CoursesChannelUserAssigner _channelUserAssigner;
    private readonly CoursesRepository _coursesRepository;
    private readonly CoursesInteractivityFormatter _coursesInteractivityFormatter;
    private readonly InMemoryDataService<Snowflake, CoursesAssignMessage> _memoryDataService;
    private readonly LocalizedStringLocalizer<CoursesPlugin> _localizer;
    private readonly IJobQueue<RemoveMessageProcessor.RemoveMessage> _queue;
    private readonly ICurrentPluginLifetime _lifetime;
    private readonly FeedbackData _feedbackData;

    /// <summary>
    /// Initializes a new instance of the <see cref="CourseMessageInteractivity"/> class.
    /// </summary>
    /// <param name="feedbackService">The feedback service.</param>
    /// <param name="commandContext">The command context.</param>
    /// <param name="interactionApi">The interaction api.</param>
    /// <param name="channelUserAssigner">The channel user assigner.</param>
    /// <param name="coursesRepository">The courses repository.</param>
    /// <param name="coursesInteractivityFormatter">The courses interactivity formatter.</param>
    /// <param name="memoryDataService">The memory data service.</param>
    /// <param name="localizer">The localizer.</param>
    /// <param name="queue">The remove message queue.</param>
    /// <param name="lifetime">The plugin lifetime.</param>
    public CourseMessageInteractivity
    (
        FeedbackService feedbackService,
        IInteractionContext commandContext,
        IDiscordRestInteractionAPI interactionApi,
        CoursesChannelUserAssigner channelUserAssigner,
        CoursesRepository coursesRepository,
        CoursesInteractivityFormatter coursesInteractivityFormatter,
        InMemoryDataService<Snowflake, CoursesAssignMessage> memoryDataService,
        LocalizedStringLocalizer<CoursesPlugin> localizer,
        IJobQueue<RemoveMessageProcessor.RemoveMessage> queue,
        ICurrentPluginLifetime lifetime
    )
    {
        _commandContext = commandContext;
        _channelUserAssigner = channelUserAssigner;
        _coursesRepository = coursesRepository;
        _coursesInteractivityFormatter = coursesInteractivityFormatter;
        _memoryDataService = memoryDataService;
        _localizer = localizer;
        _queue = queue;
        _lifetime = lifetime;
        _feedbackData = new FeedbackData(commandContext, interactionApi, feedbackService);
    }

    /// <summary>
    /// Send course message with the given assignments.
    /// </summary>
    /// <param name="prefix">The prefix to be before the message.</param>
    /// <param name="courseAssignments">The course assignments.</param>
    /// <param name="ct">The cancellation token for cancelling the operation.</param>
    /// <returns>A result that may or may not have succeeded..</returns>
    public async Task<IResult> SendCoursesMessagesAsync
        (string prefix, IReadOnlyList<CourseAssignment> courseAssignments, CancellationToken ct)
    {
        if (!_commandContext.TryGetUserID(out var userId))
        {
            return (Result)new GenericError("Could not get user id from context.");
        }

        var joinedCoursesResult = await _coursesRepository.JoinWithUserData
            (courseAssignments, userId.Value, ct);

        if (!joinedCoursesResult.IsDefined(out var joinedCourses))
        {
            return joinedCoursesResult;
        }

        var formattedMessages = _coursesInteractivityFormatter.FormatCoursesMessage(prefix, joinedCourses);
        var sentMessagesResult = await _feedbackData.SendContextualMessageDataAsync(formattedMessages, true, ct);
        if (!sentMessagesResult.IsDefined(out var sentMessages))
        {
            return sentMessagesResult;
        }

        if (formattedMessages.Count != sentMessages.Count)
        {
            // uuuuuuh
            return Result.FromError
                (new InvalidOperationError("Sent messages count does not equal formatted message count."));
        }

        var now = DateTime.Now;
        for (int i = 0; i < sentMessages.Count; i++)
        {
            var formattedMessage = formattedMessages[i];
            var sentMessage = sentMessages[i];

            if (formattedMessage.Courses is null)
            {
                continue;
            }

            if (!_memoryDataService.TryAddData
                (
                    sentMessage.ID,
                    new CoursesAssignMessage
                    (
                        sentMessage.ChannelID,
                        sentMessage.ID,
                        prefix,
                        formattedMessage.Courses.Select(x => x.CourseKey).ToArray(),
                        _localizer.Culture
                    )
                ))
            {
                return Result.FromError
                    (new InvalidOperationError("Could not add course message data to the memory service."));
            }

            _queue.EnqueueJob(new RemoveMessageProcessor.RemoveMessage(sentMessage.ID, now));
        }

        return Result.FromSuccess();
    }

    /// <summary>
    /// Handles join/leave/toggle command and if possible, edits the message the user clicked on.
    /// </summary>
    /// <param name="commandType">The type of the command to execute.</param>
    /// <param name="courses">The courses.</param>
    /// <param name="ct">The cancellation token for cancelling the operation.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    public async Task<IResult> HandleCourseCommand
    (
        InteractivityCommandType commandType,
        IEnumerable<string> courses,
        CancellationToken ct
    )
    {
        if (!_commandContext.TryGetUserID(out var userId))
        {
            return (Result)new GenericError("Could not get user id from context.");
        }

        var discordUser = new DiscordUser(userId.Value);
        CoursesAssignmentResult coursesAssignmentResult;

        switch (commandType)
        {
            case InteractivityCommandType.Join:
                coursesAssignmentResult = await _channelUserAssigner.AssignCourses
                    (discordUser, courses, ct);
                break;
            case InteractivityCommandType.Leave:
                coursesAssignmentResult = await _channelUserAssigner.DeassignCourses
                    (discordUser, courses, ct);
                break;
            case InteractivityCommandType.Toggle:
                coursesAssignmentResult = await _channelUserAssigner.ToggleCourses
                    (discordUser, courses, ct);
                break;
            default:
                throw new InvalidOperationException("Uh, what happened?");
        }

        CoursesAssignMessage? assignMessage = null;
        if (_commandContext.Interaction.Message.IsDefined(out var message))
        {
            var leasedResult = await _memoryDataService.LeaseDataAsync(message.ID, ct);
            if (leasedResult.IsDefined(out var leased))
            {
                assignMessage = leased.Data;
                await leased.DisposeAsync();
            }
        }

        if (assignMessage is null)
        {
            return await SendFeedback
            (
                _localizer,
                coursesAssignmentResult,
                _feedbackData.FeedbackService,
                ct
            );
        }

        var courseAssignmentsResult = await _coursesRepository.GetCourseAssignments
            (ct, assignMessage.Courses);

        if (!courseAssignmentsResult.IsDefined(out var courseAssignments))
        {
            return Result.FromError(courseAssignmentsResult);
        }

        var joinedCoursesResult = await _coursesRepository.JoinWithUserData
            (courseAssignments, userId.Value, ct);

        if (!joinedCoursesResult.IsDefined(out var joinedCourses))
        {
            return Result.FromError(joinedCoursesResult);
        }

        var formattedMessages = _coursesInteractivityFormatter.FormatCoursesMessage
            (assignMessage.Prepend, joinedCourses);
        return await _feedbackData.SendContextualMessageDataAsync(formattedMessages, true, ct);
    }

    /// <summary>
    /// Send feedback messages to the user containing information about assigned courses.
    /// </summary>
    /// <param name="localizer">The string localizer.</param>
    /// <param name="coursesAssignmentResult">The courses results.</param>
    /// <param name="feedbackService">The feedback service.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    public static async Task<IResult> SendFeedback
    (
        LocalizedStringLocalizer<CoursesPlugin> localizer,
        CoursesAssignmentResult coursesAssignmentResult,
        FeedbackService feedbackService,
        CancellationToken ct
    )
    {
        var errors = coursesAssignmentResult.ErrorfulResults.Values.ToList();

        if (coursesAssignmentResult.MissingCourses.Count == 0 && coursesAssignmentResult.ErrorfulResults.Count == 0
            && coursesAssignmentResult.AssignedCourses.Count == 0
            && coursesAssignmentResult.DeassignedCourses.Count == 0)
        {
            await feedbackService.SendContextualWarningAsync
                (localizer.Translate("COURSES_NOT_FOUND"), ct: ct);
        }

        if (coursesAssignmentResult.AssignedCourses.Count > 0)
        {
            var feedbackResult = await feedbackService.SendContextualSuccessAsync
            (
                $"{localizer.Translate($"COURSES_SUCCESSFULLY_ASSIGNED")}: \n"
                + CoursesFormatter.FormatCoursesMessage(coursesAssignmentResult.AssignedCourses),
                options: new FeedbackMessageOptions(MessageFlags: MessageFlags.Ephemeral),
                ct: ct
            );

            if (!feedbackResult.IsSuccess)
            {
                errors.Add(Result.FromError(feedbackResult));
            }
        }

        if (coursesAssignmentResult.DeassignedCourses.Count > 0)
        {
            var feedbackResult = await feedbackService.SendContextualSuccessAsync
            (
                $"{localizer.Translate($"COURSES_SUCCESSFULLY_DEASSIGNED")}: \n"
                + CoursesFormatter.FormatCoursesMessage(coursesAssignmentResult.DeassignedCourses),
                options: new FeedbackMessageOptions(MessageFlags: MessageFlags.Ephemeral),
                ct: ct
            );

            if (!feedbackResult.IsSuccess)
            {
                errors.Add(Result.FromError(feedbackResult));
            }
        }

        if (coursesAssignmentResult.MissingCourses.Count > 0)
        {
            var feedbackResult = await feedbackService.SendContextualWarningAsync
            (
                localizer.Translate
                (
                    "COURSES_MISSING",
                    string.Join
                        (", ", coursesAssignmentResult.MissingCourses)
                ),
                options: new FeedbackMessageOptions(MessageFlags: MessageFlags.Ephemeral),
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
            (
                localizer.Translate("ERROR"),
                options: new FeedbackMessageOptions(MessageFlags: MessageFlags.Ephemeral),
                ct: ct
            );

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
}