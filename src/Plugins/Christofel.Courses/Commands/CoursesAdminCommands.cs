//
//   CoursesAdminCommands.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Christofel.BaseLib.Extensions;
using Christofel.CommandsLib.Permissions;
using Christofel.Courses.Data;
using Christofel.Courses.Interactivity;
using Christofel.CoursesLib.Database;
using Christofel.CoursesLib.Services;
using Christofel.Helpers.Helpers;
using Christofel.Helpers.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Feedback.Messages;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Gateway.Responders;
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
    private readonly CoursesChannelCreator _channelCreator;
    private readonly FeedbackService _feedbackService;
    private readonly IDiscordRestChannelAPI _channelApi;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoursesAdminCommands"/> class.
    /// </summary>
    /// <param name="channelCreator">The courses channel creator.</param>
    /// <param name="feedbackService">The feedback service.</param>
    /// <param name="channelApi">The discord rest channel api.</param>
    public CoursesAdminCommands
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
    /// Handles removal of permissions.
    /// </summary>
    /// <param name="channelId">The id of the channel to remove permissions from.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    [Command("removepermissions")]
    public async Task<IResult> HandleRemovePermissionsAsync([DiscordTypeHint(TypeHint.Channel)] Snowflake channelId)
    {
        var channelResult = await _channelApi.GetChannelAsync(channelId, ct: CancellationToken);

        if (!channelResult.IsDefined(out var channel))
        {
            await _feedbackService.SendContextualErrorAsync("Could not get the channel.");
            return channelResult;
        }

        if (!channel.PermissionOverwrites.IsDefined(out var overwrites))
        {
            await _feedbackService.SendContextualErrorAsync("Could not find overwrites.");
            return (Result)new InvalidOperationError("Overwrites are empty.");
        }

        var removeOverwrites = overwrites
            .Where(x => x.Allow.Value == 0 && x.Type == PermissionOverwriteType.Member)
            .ToArray();
        await _feedbackService.SendContextualInfoAsync
            ($"There is {overwrites.Count}. Will remove {removeOverwrites.Length} overwrites.");

        foreach (var removeOverwrite in removeOverwrites)
        {
            var deleteResult = await _channelApi.DeleteChannelPermissionAsync
                (channelId, removeOverwrite.ID, "Deny overwrite is not useful.");
            if (!deleteResult.IsSuccess)
            {
                await _feedbackService.SendContextualErrorAsync($"Could not modify <@{removeOverwrite.ID}>.");
                return deleteResult;
            }
        }

        return await _feedbackService.SendContextualSuccessAsync
            ($"Done. Removed {removeOverwrites.Length} overwrites.");
    }

    /// <summary>
    /// A command group for /coursesadmin interactivity subcommand.
    /// </summary>
    [Group("interactivity")]
    public class Interactivity : CommandGroup
    {
        private readonly ICommandContext _commandContext;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly CoursesInteractivityFormatter _coursesInteractivityFormatter;
        private readonly FeedbackService _feedbackService;
        private readonly IDbContextFactory<CoursesContext> _coursesContext;
        private readonly ILogger<CoursesAdminCommands> _logger;
        private readonly InteractivityCultureProvider _cultureProvider;
        private readonly LocalizationOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="Interactivity"/> class.
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        /// <param name="channelApi">The discord rest channel api.</param>
        /// <param name="coursesInteractivityFormatter">The courses interactivity responder.</param>
        /// <param name="feedbackService">The feedback service.</param>
        /// <param name="options">The options.</param>
        /// <param name="coursesContext">The courses context factory.</param>
        /// <param name="cultureProvider">The culture provider.</param>
        /// <param name="logger">The logger.</param>
        public Interactivity
        (
            ICommandContext commandContext,
            IDiscordRestChannelAPI channelApi,
            CoursesInteractivityFormatter coursesInteractivityFormatter,
            FeedbackService feedbackService,
            IOptionsSnapshot<LocalizationOptions> options,
            IDbContextFactory<CoursesContext> coursesContext,
            InteractivityCultureProvider cultureProvider,
            ILogger<CoursesAdminCommands> logger
        )
        {
            _commandContext = commandContext;
            _channelApi = channelApi;
            _coursesInteractivityFormatter = coursesInteractivityFormatter;
            _feedbackService = feedbackService;
            _coursesContext = coursesContext;
            _logger = logger;
            _cultureProvider = cultureProvider;
            _options = options.Value;
        }

        /// <summary>
        /// Send the main interactivity message.
        /// </summary>
        /// <param name="language">The language of the message.</param>
        /// <param name="channel">The channel to send the message to.</param>
        /// <returns>A result that may or may not have succeeded.</returns>
        [Command("send")]
        [Description("Send the interactivity main message.")]
        public async Task<IResult> HandleSendInteractivityAsync
        (
            [Description("The language of the main message.")]
            string language,
            [Description("The channel to send the message to. (default current channel)")]
            Snowflake? channel = default
        )
        {
            _cultureProvider.CurrentCulture = language;
            if (channel is null)
            {
                _commandContext.TryGetChannelID(out channel);
            }

            if (channel is null)
            {
                return (Result)new GenericError("Could not find channel id of the context");
            }

            var mainMessage = _coursesInteractivityFormatter.FormatMainMessage
                (string.Empty, _options.SupportedLanguages);
            var messageResult = await _channelApi.CreateMessageAsync
            (
                channel.Value,
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
        /// Send the main interactivity message.
        /// </summary>
        /// <param name="messageId">The id of the message.</param>
        /// <param name="language">The language of the message.</param>
        /// <param name="channel">The channel to send the message to.</param>
        /// <returns>A result that may or may not have succeeded.</returns>
        [Command("edit")]
        [Description("Send the interactivity main message.")]
        public async Task<IResult> HandleEditInteractivityAsync
        (
            [Description("The id of the message to edit.")]
            Snowflake messageId,
            [Description("The language of the main message.")]
            string language,
            [Description("The channel the message is in. (default this channel)")]
            Snowflake? channel = default
        )
        {
            _cultureProvider.CurrentCulture = language;
            var channelId = channel ?? null;
            if (channelId is null)
            {
                _commandContext.TryGetChannelID(out channelId);
            }

            if (channelId is null)
            {
                return (Result)new GenericError("Could not find channel id of the context");
            }

            var mainMessage = _coursesInteractivityFormatter.FormatMainMessage
                (string.Empty, _options.SupportedLanguages);
            var messageResult = await _channelApi.EditMessageAsync
            (
                channelId.Value,
                messageId,
                mainMessage.Content,
                components: new Optional<IReadOnlyList<IMessageComponent>?>(mainMessage.Components),
                ct: CancellationToken
            );

            if (!messageResult.IsSuccess)
            {
                await _feedbackService.SendContextualErrorAsync
                    ($"Could not send the message. {messageResult.Error.Message}");
                return messageResult;
            }

            return await _feedbackService.SendContextualSuccessAsync("The message was edited.");
        }

        /// <summary>
        /// Tries to sync users courses in the database, just a temporary command.
        /// </summary>
        /// <returns>A result that may or may not have succeeded.</returns>
        [Command("syncassignments")]
        [Obsolete]
        public async Task<IResult> HandleSyncChannelAssignments()
        {
            await _feedbackService.SendContextualInfoAsync("Okay.");
            await using (var context = await _coursesContext.CreateDbContextAsync())
            {
                foreach (var courseAssignment in await context.CourseAssignments.ToListAsync(CancellationToken))
                {
                    try
                    {
                        var channelResult = await _channelApi.GetChannelAsync
                            (courseAssignment.ChannelId, CancellationToken);

                        if (!channelResult.IsDefined(out var channel))
                        {
                            _logger.LogResultError(channelResult);
                            continue;
                        }

                        if (!channel.PermissionOverwrites.IsDefined(out var permissions))
                        {
                            continue;
                        }

                        foreach (var permissionOverwrite in permissions)
                        {
                            var hasViewPermission = permissionOverwrite.Allow.HasPermission
                                    (DiscordPermission.ViewChannel)
                                && permissionOverwrite.Type == PermissionOverwriteType.Member;

                            if (hasViewPermission)
                            {
                                context.Add
                                (
                                    new CourseUser
                                    {
                                        CourseKey = courseAssignment.CourseKey,
                                        UserDiscordId = permissionOverwrite.ID
                                    }
                                );
                            }
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
        /// Updates the given link of course-channel.
        /// </summary>
        /// <returns>A result that may or may not have succeeded.</returns>
        /// <param name="courseKey">The key of the course to link.</param>
        /// <param name="channelId">The id of the channel to link the course to.</param>
        /// <param name="roleId">The id of the role that gives the channel.</param>
        [Command("update")]
        [Description("Updates a link for a course.")]
        public async Task<IResult> HandleUpdateAsync
        (
            string courseKey,
            [DiscordTypeHint(TypeHint.Channel)]
            Snowflake channelId,
            [DiscordTypeHint(TypeHint.Role)]
            Snowflake? roleId = null
        )
        {
            var additionResult = await _coursesChannelCreator.UpdateCourseLink
                (courseKey, channelId, roleId, CancellationToken);

            if (!additionResult.IsSuccess)
            {
                await _feedbackService.SendContextualErrorAsync
                    ($"Could not update the given link. {additionResult.Error.Message}");
                return additionResult;
            }

            return await _feedbackService.SendContextualSuccessAsync("Successfully updated the link.");
        }

        /// <summary>
        /// Adds the given link of course-channel.
        /// </summary>
        /// <returns>A result that may or may not have succeeded.</returns>
        /// <param name="courseKey">The key of the course to link.</param>
        /// <param name="channelId">The id of the channel to link the course to.</param>
        /// <param name="roleId">The id of the role that gives the channel.</param>
        [Command("add")]
        [Description("Adds a link for a course to given channel.")]
        public async Task<IResult> HandleAddAsync
        (
            string courseKey,
            [DiscordTypeHint(TypeHint.Channel)]
            Snowflake channelId,
            [DiscordTypeHint(TypeHint.Role)]
            Snowflake? roleId = null
        )
        {
            var additionResult = await _coursesChannelCreator.CreateCourseLink
                (courseKey, channelId, roleId, CancellationToken);

            if (!additionResult.IsSuccess)
            {
                await _feedbackService.SendContextualErrorAsync
                    ($"Could not create the given link. {additionResult.Error.Message}");
                return additionResult;
            }

            return await _feedbackService.SendContextualSuccessAsync("Successfully created the link.");
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
                    string.Join
                    (
                        '\n',
                        courses
                            .Select
                            (
                                x =>
                                {
                                    var roleString = x.RoleId is not null ? $"(<@&{x.RoleId}>)" : string.Empty;
                                    return
                                        $"  {x.CourseName} ({x.CourseKey}) {roleString}";
                                }
                            )
                    ),
                    options: new FeedbackMessageOptions(AllowedMentions: AllowedMentionsHelper.None)
                );
        }
    }
}