//
//   CustomVoiceCommandGroup.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Christofel.BaseLib.Extensions;
using Christofel.CommandsLib.Extensions;
using Christofel.CommandsLib.Permissions;
using Christofel.CommandsLib.Validator;
using Christofel.Common.Database.Models.Enums;
using FluentValidation;
using OneOf;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Commands.Results;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.Enhancements.CustomVoice;

/// <summary>
/// Commands for managing custom voice channels.
/// </summary>
[Group("customvoice")]
[Ephemeral]
[RequirePermission("enhancements.customvoice.commands")]
public class CustomVoiceCommandGroup : CommandGroup
{
    private readonly CustomVoiceService _customVoiceService;
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly ICommandContext _commandContext;
    private readonly FeedbackService _feedbackService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomVoiceCommandGroup"/> class.
    /// </summary>
    /// <param name="customVoiceService">The custom voice service.</param>
    /// <param name="channelApi">The channel api.</param>
    /// <param name="commandContext">The command context.</param>
    /// <param name="feedbackService">The feedback service.</param>
    public CustomVoiceCommandGroup
    (
        CustomVoiceService customVoiceService,
        IDiscordRestChannelAPI channelApi,
        ICommandContext commandContext,
        FeedbackService feedbackService
    )
    {
        _customVoiceService = customVoiceService;
        _channelApi = channelApi;
        _commandContext = commandContext;
        _feedbackService = feedbackService;
    }

    /// <summary>
    /// Renames the custom voice.
    /// </summary>
    /// <param name="name">The name to rename to.</param>
    /// <returns>A result that may have failed.</returns>
    [Command("rename")]
    [RequirePermission("enhancements.customvoice.commands.rename")]
    [Description("Rename the custom voice channel to the given name.")]
    public async Task<Result> HandleRenameAsync(string name)
    {
        var customVoice = _customVoiceService.GetChannelUserIsConnectedTo(_commandContext.User.ID);
        var validationResult = new CommandValidator()
            .MakeSure("voice", customVoice is null ? string.Empty : "notempty", o => o.NotEmpty())
            .MakeSure("name", name, o => o.MaximumLength(100))
            .Validate()
            .GetResult();

        if (!validationResult.IsSuccess || customVoice is null)
        {
            return validationResult;
        }

        var permissionsResult = await CheckPermissions
        (
            _commandContext,
            _customVoiceService,
            customVoice,
            _feedbackService,
            CancellationToken
        );
        if (!permissionsResult.IsSuccess)
        {
            return permissionsResult;
        }

        var modificationResult = await _channelApi.ModifyChannelAsync(customVoice.ChannelId, name, ct: CancellationToken);

        if (!modificationResult.IsSuccess)
        {
            await _feedbackService.SendContextualErrorAsync($"Could not rename the channel.");
            return Result.FromError(modificationResult);
        }

        await _feedbackService.SendContextualSuccessAsync($"Channel renamed to {name}.");
        return Result.FromSuccess();
    }

    private static async Task<Result> CheckPermissions
    (
        ICommandContext commandContext,
        CustomVoiceService customVoiceService,
        CustomVoiceChannel customVoice,
        FeedbackService feedbackService,
        CancellationToken ct
    )
    {
        var permissionResult = await customVoiceService.IsPermittedToChangeChannel
            (commandContext.User.ID, customVoice, ct);
        if (!permissionResult.IsDefined(out var permission))
        {
            return Result.FromError(permissionResult);
        }

        if (!permission)
        {
            await feedbackService.SendContextualErrorAsync("Insufficient permissions.", ct: ct);
            return new PermissionDeniedError("The user does not control the custom voice he is in.");
        }

        return Result.FromSuccess();
    }

    private static async Task<Result<CustomVoiceChannel>> LoadCustomVoiceAndCheckPermissions
    (
        ICommandContext commandContext,
        CustomVoiceService customVoiceService,
        FeedbackService feedbackService,
        CancellationToken ct
    )
    {
        var customVoice = customVoiceService.GetChannelUserIsConnectedTo(commandContext.User.ID);
        var validationResult = new CommandValidator()
            .MakeSure("voice", customVoice is null ? string.Empty : "notempty", o => o.NotEmpty())
            .Validate()
            .GetResult();

        if (!validationResult.IsSuccess || customVoice is null)
        {
            return Result<CustomVoiceChannel>.FromError(validationResult);
        }

        var permissionsResult = await CheckPermissions
        (
            commandContext,
            customVoiceService,
            customVoice,
            feedbackService,
            ct
        );
        if (!permissionsResult.IsSuccess)
        {
            return Result<CustomVoiceChannel>.FromError(permissionsResult);
        }

        return customVoice;
    }

    /// <summary>
    /// Subcommand for managing custom voice moderators.
    /// </summary>
    [Group("moderator")]
    public class ModeratorSubCommand : CommandGroup
    {
        private readonly CustomVoiceService _customVoiceService;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly ICommandContext _commandContext;
        private readonly FeedbackService _feedbackService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModeratorSubCommand"/> class.
        /// </summary>
        /// <param name="customVoiceService">The custom voice service.</param>
        /// <param name="channelApi">The channel api.</param>
        /// <param name="commandContext">The command context.</param>
        /// <param name="feedbackService">The feedback service.</param>
        public ModeratorSubCommand
        (
            CustomVoiceService customVoiceService,
            IDiscordRestChannelAPI channelApi,
            ICommandContext commandContext,
            FeedbackService feedbackService
        )
        {
            _customVoiceService = customVoiceService;
            _channelApi = channelApi;
            _commandContext = commandContext;
            _feedbackService = feedbackService;
        }

        /// <summary>
        /// Add the user or role as a moderator.
        /// </summary>
        /// <remarks>
        /// The user or role will be able to mute, move and deafen members.
        /// </remarks>
        /// <param name="userOrRole">The user or role.</param>
        /// <returns>A result that may have failed.</returns>
        [Command("add")]
        [RequirePermission("enhancements.customvoice.commands.moderator.add")]
        [Description("Add moderator permissions to the given user.")]
        public async Task<Result> HandleAddAsync
            ([DiscordTypeHint(TypeHint.Mentionable)] OneOf<IPartialGuildMember, IRole> userOrRole)
        {
            var customVoiceResult = await LoadCustomVoiceAndCheckPermissions
                (_commandContext, _customVoiceService, _feedbackService, CancellationToken);
            if (!customVoiceResult.IsDefined(out var customVoice))
            {
                return Result.FromError(customVoiceResult);
            }
            var discordTarget = userOrRole.ToDiscordTarget(true);

            var editResult = await _channelApi.EditChannelPermissionsAsync
            (
                customVoice.ChannelId,
                discordTarget.DiscordId,
                new DiscordPermissionSet
                (
                    DiscordPermission.MuteMembers,
                    DiscordPermission.MoveMembers,
                    DiscordPermission.DeafenMembers,
                    DiscordPermission.ViewChannel,
                    DiscordPermission.Connect,
                    DiscordPermission.Speak,
                    DiscordPermission.RequestToSpeak,
                    DiscordPermission.SendMessages
                ),
                default,
                discordTarget.TargetType == TargetType.Role ? PermissionOverwriteType.Role : PermissionOverwriteType.Member,
                "Addition of a moderator to custom voice upon request."
            );

            if (editResult.IsSuccess)
            {
                var feedbackResult = await _feedbackService.SendContextualSuccessAsync
                    ($"Added the target {discordTarget.GetMentionString()} as a moderator.");
                return feedbackResult.IsSuccess
                    ? Result.FromSuccess()
                    : Result.FromError(feedbackResult);
            }

            await _feedbackService.SendContextualSuccessAsync
                ($"Could not add the target {discordTarget.GetMentionString()} as a moderator.");
            return editResult;
        }

        /// <summary>
        /// Remove the user or role as a moderator.
        /// </summary>
        /// <remarks>
        /// The user or role will no longer be able to mute, move and deafen members.
        /// </remarks>
        /// <param name="userOrRole">The user or role.</param>
        /// <returns>A result that may have failed.</returns>
        [Command("remove")]
        [RequirePermission("enhancements.customvoice.commands.moderator.remove")]
        [Description("Remove moderator permissions from the given user.")]
        public async Task<Result> HandleRemoveAsync
            ([DiscordTypeHint(TypeHint.Mentionable)] OneOf<IPartialGuildMember, IRole> userOrRole)
        {
            if (userOrRole.IsT0 && userOrRole.AsT0.User.IsDefined(out var user) && user.ID == _commandContext.User.ID)
            {
                return Result.FromSuccess();
            }

            var customVoiceResult = await LoadCustomVoiceAndCheckPermissions
                (_commandContext, _customVoiceService, _feedbackService, CancellationToken);
            if (!customVoiceResult.IsDefined(out var customVoice))
            {
                return Result.FromError(customVoiceResult);
            }
            var discordTarget = userOrRole.ToDiscordTarget(true);

            var editResult = await _channelApi.EditChannelPermissionsAsync
            (
                customVoice.ChannelId,
                discordTarget.DiscordId,
                default,
                default,
                discordTarget.TargetType == TargetType.Role ? PermissionOverwriteType.Role : PermissionOverwriteType.Member,
                "Removal of a moderator to custom voice upon request."
            );

            if (editResult.IsSuccess)
            {
                var feedbackResult = await _feedbackService.SendContextualSuccessAsync
                    ($"The target {discordTarget.GetMentionString()} is no longer a moderator.");
                return feedbackResult.IsSuccess
                    ? Result.FromSuccess()
                    : Result.FromError(feedbackResult);
            }

            await _feedbackService.SendContextualSuccessAsync
                ($"Could not remove the target {discordTarget.GetMentionString()} from being a moderator.");
            return editResult;
        }
    }

    /// <summary>
    /// Manages access to the channel.
    /// </summary>
    [Group("access")]
    public class AccessSubCommand : CommandGroup
    {
        private readonly CustomVoiceService _customVoiceService;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly ICommandContext _commandContext;
        private readonly FeedbackService _feedbackService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessSubCommand"/> class.
        /// </summary>
        /// <param name="customVoiceService">The custom voice service.</param>
        /// <param name="channelApi">The channel api.</param>
        /// <param name="commandContext">The command context.</param>
        /// <param name="feedbackService">The feedback service.</param>
        public AccessSubCommand
        (
            CustomVoiceService customVoiceService,
            IDiscordRestChannelAPI channelApi,
            ICommandContext commandContext,
            FeedbackService feedbackService
        )
        {
            _customVoiceService = customVoiceService;
            _channelApi = channelApi;
            _commandContext = commandContext;
            _feedbackService = feedbackService;
        }

        /// <summary>
        /// Grants access to the given user or a role.
        /// </summary>
        /// <param name="userOrRole">The user or a role.</param>
        /// <returns>A result that may have failed.</returns>
        [Command("grant")]
        [RequirePermission("enhancements.customvoice.commands.access.grant")]
        [Description("Grant access to the given user or role.")]
        public async Task<Result> HandleGrantAsync
            ([DiscordTypeHint(TypeHint.Mentionable)] OneOf<IPartialGuildMember, IRole> userOrRole)
        {
            var customVoiceResult = await LoadCustomVoiceAndCheckPermissions
                (_commandContext, _customVoiceService, _feedbackService, CancellationToken);
            if (!customVoiceResult.IsDefined(out var customVoice))
            {
                return Result.FromError(customVoiceResult);
            }
            var discordTarget = userOrRole.ToDiscordTarget(true);

            var editResult = await _channelApi.EditChannelPermissionsAsync
            (
                customVoice.ChannelId,
                discordTarget.DiscordId,
                new DiscordPermissionSet
                (
                    DiscordPermission.ViewChannel,
                    DiscordPermission.Connect,
                    DiscordPermission.Speak,
                    DiscordPermission.RequestToSpeak,
                    DiscordPermission.SendMessages
                ),
                default,
                discordTarget.TargetType == TargetType.Role ? PermissionOverwriteType.Role : PermissionOverwriteType.Member,
                "Addition of a moderator to custom voice upon request."
            );

            if (editResult.IsSuccess)
            {
                var feedbackResult = await _feedbackService.SendContextualSuccessAsync
                            ($"Granted permissions to access to the given target {discordTarget.GetMentionString()}.");
                return feedbackResult.IsSuccess
                    ? Result.FromSuccess()
                    : Result.FromError(feedbackResult);
            }

            await _feedbackService.SendContextualSuccessAsync
                ($"Could not grant permissions to access to {discordTarget.GetMentionString()}.");
            return editResult;
        }
    }
}