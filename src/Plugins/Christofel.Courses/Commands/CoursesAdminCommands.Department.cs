//
//   CoursesAdminCommands.Department.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Christofel.CoursesLib.Services;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
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
}