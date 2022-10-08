//
//   CoursesAdminCommands.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Christofel.CommandsLib.Permissions;
using Christofel.Courses.Interactivity;
using Christofel.CoursesLib.Database;
using Christofel.CoursesLib.Services;
using Christofel.Helpers.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.Courses.Commands;

/// <summary>
/// A class for /coursesadmin command group.
/// </summary>
[Group("coursesadmin")]
[RequirePermission("courses.coursesadmin")]
[Ephemeral]
public class CoursesAdminCommands : CommandGroup
{
    private readonly ICommandContext _commandContext;
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly CoursesInteractivityFormatter _coursesInteractivityFormatter;
    private readonly CoursesChannelCreator _channelCreator;
    private readonly FeedbackService _feedbackService;
    private readonly IDbContextFactory<CoursesContext> _coursesContext;
    private readonly ILogger<CoursesAdminCommands> _logger;
    private readonly LocalizationOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoursesAdminCommands"/> class.
    /// </summary>
    /// <param name="commandContext">The command context.</param>
    /// <param name="channelApi">The discord rest channel api.</param>
    /// <param name="coursesInteractivityFormatter">The courses interactivity responder.</param>
    /// <param name="channelCreator">The courses channel creator.</param>
    /// <param name="feedbackService">The feedback service.</param>
    /// <param name="options">The options.</param>
    /// <param name="coursesContext">The courses context factory.</param>
    /// <param name="logger">The logger.</param>
    public CoursesAdminCommands
    (
        ICommandContext commandContext,
        IDiscordRestChannelAPI channelApi,
        CoursesInteractivityFormatter coursesInteractivityFormatter,
        CoursesChannelCreator channelCreator,
        FeedbackService feedbackService,
        IOptionsSnapshot<LocalizationOptions> options,
        IDbContextFactory<CoursesContext> coursesContext,
        ILogger<CoursesAdminCommands> logger
    )
    {
        _commandContext = commandContext;
        _channelApi = channelApi;
        _coursesInteractivityFormatter = coursesInteractivityFormatter;
        _channelCreator = channelCreator;
        _feedbackService = feedbackService;
        _coursesContext = coursesContext;
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// Creates a channel for a given course.
    /// </summary>
    /// <param name="courseKey">The key of the course.</param>
    /// <param name="channelName">The name of the channel to create (otherwise).</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    [Command("create")]
    public async Task<IResult> HandleCreateAsync(string courseKey, string? channelName = default)
    {
        var result = await _channelCreator.CreateCourseChannel(courseKey, channelName, CancellationToken);

        if (result.IsSuccess)
        {
            return await _feedbackService.SendContextualSuccessAsync("Successfully created a new channel.");
        }

        await _feedbackService.SendContextualErrorAsync($"Could not create the channel. {result.Error.Message}");
        return result;
    }

    /// <summary>
    /// Send the main interactivity message.
    /// </summary>
    /// <param name="language">The language of the message.</param>
    /// <param name="channel">The channel to send the message to.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    [Command("interactivity")]
    [Description("Send the interactivity main message.")]
    public async Task<IResult> HandleSendInteractivityAsync
    (
        [Description("The language of the main message.")]
        string language,
        [Description("The channel to send the message to. (default current channel)")]
        Snowflake? channel = default
    )
    {
        var channelId = channel ?? _commandContext.ChannelID;
        var mainMessage = _coursesInteractivityFormatter.FormatMainMessage
            (string.Empty, language, _options.SupportedLanguages);
        var messageResult = await _channelApi.CreateMessageAsync
        (
            channelId,
            mainMessage.Content,
            components: new Optional<IReadOnlyList<IMessageComponent>>(mainMessage.Components),
            ct: CancellationToken
        );

        if (!messageResult.IsSuccess)
        {
            await _feedbackService.SendContextualErrorAsync
                ($"Could not send the message. {messageResult.Error.Message}");
            return messageResult;
        }

        return await _feedbackService.SendContextualSuccessAsync("The message was sent.");
    }

    /// <summary>
    /// Tries to solve inconsistencies in the database, just a temporary command.
    /// </summary>
    /// <returns>A result that may or may not have succeeded.</returns>
    [Command("inconsistencies")]
    [Obsolete]
    public async Task<IResult> HandleInconsistenciesAsync()
    {
        await _feedbackService.SendContextualInfoAsync("Okay.");
        await using (var context = await _coursesContext.CreateDbContextAsync())
        {
            var groupAssignments = new HashSet<ulong>();

            foreach (var courseAssignment in await context.CourseAssignments.ToListAsync(CancellationToken))
            {
                try
                {
                    if (!groupAssignments.Contains(courseAssignment.ChannelId.Value))
                    {
                        var groupAssignment = await context.CourseGroupAssignments
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x => x.ChannelId == courseAssignment.ChannelId);

                        if (groupAssignment is null)
                        {
                            groupAssignment = new CourseGroupAssignment()
                            {
                                ChannelId = courseAssignment.ChannelId
                            };

                            context.Add(groupAssignment);
                        }
                        groupAssignments.Add(groupAssignment.ChannelId.Value);
                    }

                    if (string.IsNullOrWhiteSpace(courseAssignment.ChannelName))
                    {
                        var channelResult = await _channelApi.GetChannelAsync
                            (courseAssignment.ChannelId, CancellationToken);
                        if (!channelResult.IsDefined(out var channel))
                        {
                            await _feedbackService.SendContextualWarningAsync
                            (
                                $"Could not find channel {courseAssignment.ChannelId} for course {courseAssignment.CourseKey}."
                            );
                            continue;
                        }

                        courseAssignment.ChannelName = channel.Name.HasValue ? channel.Name.Value : null;
                    }
                }
                catch (Exception e)
                {
                    await _feedbackService.SendContextualErrorAsync
                        ($"There was an error when processing {courseAssignment.CourseKey}: " + e.Message);
                    _logger.LogError(e, $"There was an error when processing {courseAssignment.CourseKey}");
                }
            }

            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                await _feedbackService.SendContextualErrorAsync
                    ($"There was an error while saving: " + e.Message);
                _logger.LogError(e, $"There was an error while saving resolved inconsistencies");
            }
        }

        await _feedbackService.SendContextualInfoAsync("Done.");
        return Result.FromSuccess();
    }

    /// <summary>
    /// A command group for /coursesadmin department subcommands.
    /// </summary>
    [Group("department")]
    public class DepartmentCommands : CommandGroup
    {
        private readonly CoursesChannelCreator _channelCreator;
        private readonly FeedbackService _feedbackService;
        private readonly IDiscordRestChannelAPI _channelApi;

        /// <summary>
        /// Initializes a new instance of the <see cref="DepartmentCommands"/> class.
        /// </summary>
        /// <param name="channelCreator">The department channel assigner.</param>
        /// <param name="feedbackService">The feedback service.</param>
        /// <param name="channelApi">The discord rest channel api.</param>
        public DepartmentCommands
        (
            CoursesChannelCreator channelCreator,
            FeedbackService feedbackService,
            IDiscordRestChannelAPI channelApi
        )
        {
            _channelCreator = channelCreator;
            _feedbackService = feedbackService;
            _channelApi = channelApi;
        }

        /// <summary>
        /// Assigns department to channel.
        /// </summary>
        /// <param name="departmentKey">The department key.</param>
        /// <param name="categoryId">The id of the category to assign.</param>
        /// <returns>A result that may or may not have succeeded.</returns>
        [Command("assign")]
        [Description("Assign given department to the given channel.")]
        public async Task<IResult> HandleAssignAsync
            (string departmentKey, [DiscordTypeHint(TypeHint.Channel)] Snowflake categoryId)
        {
            var channelResult = await _channelApi.GetChannelAsync(categoryId, CancellationToken);
            if (!channelResult.IsDefined(out var channel))
            {
                await _feedbackService.SendContextualErrorAsync("Could not load the given category.");
                return channelResult;
            }

            if (channel.Type != ChannelType.GuildCategory)
            {
                return await _feedbackService.SendContextualErrorAsync("The given category is not a category channel.");
            }

            var assignmentResult = await _channelCreator.AssignDepartmentCategory
                (departmentKey, categoryId, CancellationToken);

            if (assignmentResult.IsSuccess)
            {
                return await _feedbackService.SendContextualSuccessAsync("The category was assigned correctly.");
            }

            await _feedbackService.SendContextualErrorAsync
                ($"Could not assign the category. {assignmentResult.Error.Message}");
            return assignmentResult;
        }

        /// <summary>
        /// Deassigns department from a channel.
        /// </summary>
        /// <param name="departmentKey">The department key.</param>
        /// <returns>A result that may or may not have succeeded.</returns>
        [Command("deassign")]
        public async Task<IResult> HandleDeassignAsync(string departmentKey)
        {
            var assignmentResult = await _channelCreator.DeassignDepartmentCategory
                (departmentKey, CancellationToken);

            if (assignmentResult.IsSuccess)
            {
                return await _feedbackService.SendContextualSuccessAsync("The category was deassigned correctly.");
            }

            await _feedbackService.SendContextualErrorAsync
                ($"Could not deassign the category. {assignmentResult.Error.Message}");
            return assignmentResult;
        }
    }

    /// <summary>
    /// A command group for /courses link.
    /// </summary>
    [Group("link")]
    public class LinkCommands : CommandGroup
    {
        private readonly CoursesChannelCreator _coursesChannelCreator;
        private readonly CoursesRepository _coursesRepository;
        private readonly FeedbackService _feedbackService;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkCommands"/> class.
        /// </summary>
        /// <param name="coursesChannelCreator">The courses channel creator.</param>
        /// <param name="coursesRepository">The courses info.</param>
        /// <param name="feedbackService">The feedback service.</param>
        public LinkCommands
        (
            CoursesChannelCreator coursesChannelCreator,
            CoursesRepository coursesRepository,
            FeedbackService feedbackService
        )
        {
            _coursesChannelCreator = coursesChannelCreator;
            _coursesRepository = coursesRepository;
            _feedbackService = feedbackService;
        }

        /// <summary>
        /// Adds the given link of course-channel.
        /// </summary>
        /// <returns>A result that may or may not have succeeded.</returns>
        /// <param name="courseKey">The key of the course to link.</param>
        /// <param name="channelId">The id of the channel to link the course to.</param>
        [Command("add")]
        [Description("Adds a link for a course to given channel.")]
        public async Task<IResult> HandleAddAsync
            (string courseKey, [DiscordTypeHint(TypeHint.Channel)] Snowflake channelId)
        {
            var additionResult = await _coursesChannelCreator.CreateCourseLink(courseKey, channelId, CancellationToken);

            if (!additionResult.IsSuccess)
            {
                await _feedbackService.SendContextualErrorAsync
                    ($"Could not create the given link. {additionResult.Error.Message}");
                return additionResult;
            }

            return await _feedbackService.SendContextualSuccessAsync("Successfully created the link.");
        }

        /// <summary>
        /// Adds the given links of course-channel.
        /// </summary>
        /// <returns>A result that may or may not have succeeded.</returns>
        /// <param name="courseLinks">The links to create formatted such as #channel1:course1Key #channel2:course2Key.</param>
        [Command("addlist")]
        [Description("Adds multiple links in one command, for format see description of courseKeys argument.")]
        public async Task<IResult> HandleAddListAsync
        (
            [Description
                ("The courses to link in format: #channel1:course1Key #channel2:course2Key ...")]
            string courseLinks
        )
        {
            var errors = new List<IResult>();

            foreach (var courseLink in courseLinks.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                var splitted = courseLink.Split(':');
                if (splitted.Length != 2)
                {
                    await _feedbackService.SendContextualWarningAsync($"Could not parse a link: {courseLink}");
                    continue;
                }

                var channelIdString = splitted[0].Trim('<', '>').Trim('#');
                var courseKey = splitted[1];

                if (!DiscordSnowflake.TryParse(channelIdString, out var channelId))
                {
                    await _feedbackService.SendContextualWarningAsync
                        ($"Could not parse the id of the channel {channelIdString}");
                    continue;
                }

                var additionResult = await _coursesChannelCreator.CreateCourseLink
                    (courseKey, channelId.Value, CancellationToken);

                if (!additionResult.IsSuccess)
                {
                    errors.Add(additionResult);
                    await _feedbackService.SendContextualErrorAsync
                        ($"Could not create the given link {courseLink}. {additionResult.Error.Message}");
                }

                await _feedbackService.SendContextualSuccessAsync
                    ($"Successfully created the link {courseLink}.");
            }

            return errors.Count switch
            {
                0 => Result.FromSuccess(),
                1 => errors[0],
                _ => Result.FromError(new AggregateError(errors))
            };
        }

        /// <summary>
        /// Removes the given course link.
        /// </summary>
        /// <param name="courseKey">The key of the course.</param>
        /// <returns>A result that may or may not have succeeded.</returns>
        [Command("remove")]
        public async Task<IResult> HandleRemoveAsync(string courseKey)
        {
            var removalResult = await _coursesChannelCreator.RemoveCourseLink(courseKey, CancellationToken);

            if (!removalResult.IsSuccess)
            {
                await _feedbackService.SendContextualErrorAsync
                    ($"Could not remove the given link. {removalResult.Error.Message}");
                return removalResult;
            }

            return await _feedbackService.SendContextualSuccessAsync("Successfully removed the link.");
        }

        /// <summary>
        /// Lists all courses associated with the given channel.
        /// </summary>
        /// <param name="channelId">The channel id.</param>
        /// <returns>A result that may or may not have succeeded.</returns>
        [Command("list")]
        public async Task<IResult> HandleListAsync([DiscordTypeHint(TypeHint.Channel)] Snowflake channelId)
        {
            var coursesResult = await _coursesRepository.GetCoursesByChannel(channelId);

            if (!coursesResult.IsDefined(out var courses))
            {
                await _feedbackService.SendContextualErrorAsync
                    ($"Could not obtain the list. {coursesResult.Error?.Message}");
                return coursesResult;
            }

            if (courses.Count == 0)
            {
                return await _feedbackService.SendContextualInfoAsync
                    ("There aren't any courses linked with the given channel.");
            }

            return await _feedbackService.SendContextualInfoAsync
            (
                "The following courses are linked with the given channel:\n" +
                string.Join('\n', courses.Select(x => $"  {x.CourseName} ({x.CourseKey})"))
            );
        }
    }
}