//
//  ChannelRoleCommandsGroup.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.CommandsLib.Permissions;
using OneOf;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.Management.Commands;

/// <summary>
/// The command group for /channels command.
/// </summary>
[Group("channels")]
[RequirePermission("management.channels")]
[Ephemeral]
public class ChannelRoleCommandsGroup : CommandGroup
{
    private readonly FeedbackService _feedbackService;
    private readonly IDiscordRestChannelAPI _channelApi;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelRoleCommandsGroup"/> class.
    /// </summary>
    /// <param name="feedbackService">The feedback service.</param>
    /// <param name="channelApi">The channel rest api.</param>
    public ChannelRoleCommandsGroup(FeedbackService feedbackService, IDiscordRestChannelAPI channelApi)
    {
        _feedbackService = feedbackService;
        _channelApi = channelApi;
    }

    /// <summary>
    /// Handles /channels migrate subcommand.
    /// </summary>
    [Group("migrate")]
    public class Migrate : CommandGroup
    {
        private readonly FeedbackService _feedbackService;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IDiscordRestGuildAPI _guildApi;

        /// <summary>
        /// Initializes a new instance of the <see cref="Migrate"/> class.
        /// </summary>
        /// <param name="feedbackService">The feedback service.</param>
        /// <param name="channelApi">The rest channel api.</param>
        /// <param name="guildApi">The rest guild api.</param>
        public Migrate
        (
            FeedbackService feedbackService,
            IDiscordRestChannelAPI channelApi,
            IDiscordRestGuildAPI guildApi
        )
        {
            _feedbackService = feedbackService;
            _channelApi = channelApi;
            _guildApi = guildApi;
        }

        /// <summary>
        /// Handles /channels migrate role.
        /// </summary>
        /// <param name="role">The id of the role or name of a new role.</param>
        /// <param name="channel">The id of the channel to get overrides from.</param>
        /// <returns>A result that may or may not have succeeded.</returns>
        [Command("role")]
        [Description("Migrates channel overrides to a role.")]
        public async Task<IResult> HandleMigrateToRoleAsync
        (
            OneOf<Snowflake, string> role,
            [DiscordTypeHint(TypeHint.Channel)]
            Snowflake channel
        )
        {
            var channelResult = await _channelApi.GetChannelAsync(channel, CancellationToken);

            if (!channelResult.IsDefined(out var lChannel))
            {
                await _feedbackService.SendContextualErrorAsync("Could not find the channel.", ct: CancellationToken);
                return channelResult;
            }

            if (!lChannel.PermissionOverwrites.IsDefined(out var permissionOverwrites))
            {
                await _feedbackService.SendContextualInfoAsync("No overwrites found.", ct: CancellationToken);
                return Result.FromSuccess();
            }

            if (!lChannel.GuildID.IsDefined(out var guildId))
            {
                await _feedbackService.SendContextualErrorAsync
                    ("The channel is not in a guild.", ct: CancellationToken);
                return Result.FromSuccess();
            }

            if (role.TryPickT1(out string roleName, out var roleId))
            {
                var roleResult = await _guildApi.CreateGuildRoleAsync
                (
                    guildId,
                    roleName,
                    reason: "A role for migrating from channel overrides",
                    ct: CancellationToken
                );

                if (!roleResult.IsDefined(out var createdRole))
                {
                    await _feedbackService.SendContextualErrorAsync
                        ("There was an error whilst creating the role.", ct: CancellationToken);
                    return roleResult;
                }

                roleId = createdRole.ID;
            }

            int addedCount = 0;
            foreach (var overwrite in permissionOverwrites)
            {
                if (overwrite.Type == PermissionOverwriteType.Role)
                {
                    continue;
                }

                if (overwrite.Allow.Value == 0)
                {
                    continue;
                }

                var result = await _guildApi.AddGuildMemberRoleAsync
                    (guildId, overwrite.ID, roleId, "Migrate overwrites to role.");

                if (!result.IsSuccess)
                {
                    await _feedbackService.SendContextualErrorAsync
                        ("There was an error whilst adding members to the role.", ct: CancellationToken);
                    return result;
                }

                addedCount++;
            }

            await _feedbackService.SendContextualSuccessAsync
                ($"Done. Added {addedCount} members.", ct: CancellationToken);
            return Result.FromSuccess();
        }
    }
}