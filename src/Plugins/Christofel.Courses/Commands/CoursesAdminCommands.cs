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
using Remora.Discord.Commands.Extensions;
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
public partial class CoursesAdminCommands : CommandGroup
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
    /// Creates a channel for a given course.
    /// </summary>
    /// <param name="courseKey">The key of the course.</param>
    /// <param name="courseName">The name of the course.</param>
    /// <param name="departmentKey">The department key.</param>
    /// <param name="channelName">The name of the channel to create (otherwise).</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    [Command("createmanual")]
    public async Task<IResult> HandleCreateManualAsync
    (
        string courseKey,
        string courseName,
        string departmentKey,
        string? channelName = default
    )
    {
        var result = await _channelCreator.CreateCourseChannel
            (courseKey, courseName, departmentKey, channelName, CancellationToken);

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
}