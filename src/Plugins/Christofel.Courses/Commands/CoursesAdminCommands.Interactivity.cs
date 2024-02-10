//
//   CoursesAdminCommands.Interactivity.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Christofel.BaseLib.Extensions;
using Christofel.Courses.Data;
using Christofel.Courses.Interactivity;
using Christofel.CoursesLib.Database;
using Christofel.Helpers.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.Courses.Commands;

/// <summary>
/// A class for /coursesadmin command group.
/// </summary>
public partial class CoursesAdminCommands
{
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
            if (channel is null && _commandContext.TryGetChannelID(out var foundChannel))
            {
                channel = foundChannel;
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
            if (channelId is null && _commandContext.TryGetChannelID(out var foundChannel))
            {
                channelId = foundChannel;
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
}