//
//   CoursesAdminCommands.Link.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Xml.Linq;
using Christofel.CoursesLib.Services;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.Commands.Attributes;
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
        /// <param name="roleId">The role id to assign.</param>
        [Command("add")]
        [Description("Adds a link for a course to given channel.")]
        public async Task<IResult> HandleAddAsync
        (
            string courseKey,
            [DiscordTypeHint(TypeHint.Channel)] Snowflake channelId,
            [DiscordTypeHint(TypeHint.Role)] Snowflake? roleId = null
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
        /// Adds the given link of course-channel.
        /// </summary>
        /// <returns>A result that may or may not have succeeded.</returns>
        /// <param name="courseKey">The key of the course to link.</param>
        /// <param name="courseName">The name of the course.</param>
        /// <param name="departmentKey">The department key.</param>
        /// <param name="channelId">The id of the channel to link the course to.</param>
        /// <param name="roleId">The role id to assign.</param>
        [Command("addmanual")]
        [Description("Adds a link for a course to given channel, info is provided manually.")]
        public async Task<IResult> HandleAddManualAsync
        (
            string courseKey,
            [Description("Full name of the course")]
            string courseName,
            [Description("The key of the department, ie. 13101")]
            string departmentKey,
            [DiscordTypeHint(TypeHint.Channel)] Snowflake channelId,
            [DiscordTypeHint(TypeHint.Role)] Snowflake? roleId = null
        )
        {
            var additionResult = await _coursesChannelCreator.CreateCourseLink
                (courseKey, courseName, departmentKey, channelId, roleId, CancellationToken);

            if (!additionResult.IsSuccess)
            {
                await _feedbackService.SendContextualErrorAsync
                    ($"Could not create the given link. {additionResult.Error.Message}");
                return additionResult;
            }

            return await _feedbackService.SendContextualSuccessAsync("Successfully created the link.");
        }

        /// <summary>
        /// Edit the given link of course-channel.
        /// </summary>
        /// <returns>A result that may or may not have succeeded.</returns>
        /// <param name="courseKey">The key of the course to link.</param>
        /// <param name="channelId">The id of the channel to link the course to.</param>
        /// <param name="roleId">The id of the role to link the course to.</param>
        [Command("edit")]
        [Description("Adds a link for a course to given channel.")]
        public async Task<IResult> HandleEditAsync
        (
            string courseKey,
            [DiscordTypeHint(TypeHint.Channel)] Snowflake channelId,
            [DiscordTypeHint(TypeHint.Role)] Snowflake? roleId = null
        )
        {
            var additionResult = await _coursesChannelCreator.UpdateCourseLink
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
                    courses.Select
                    (
                        x => $"- {x.CourseName} ({x.CourseKey})" + (x.RoleId is not null ? $" <@&{x.RoleId}>" : string.Empty)
                    )
                )
            );
        }
    }
}