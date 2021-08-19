using System;
using Discord;
using Discord.Net.Interactions.Abstractions;
using Discord.Net.Interactions.CommandsInfo;

namespace Christofel.CommandsLib
{
    public sealed class PermissionSlashInfoBuilder
        : SlashCommandInfoBuilder<PermissionSlashInfoBuilder, PermissionSlashInfo>
    {
        public string? Permission { get; set; }
        
        public PermissionSlashInfoBuilder WithPermission(string permissionName)
        {
            Permission = permissionName;
            return this;
        }

        public override PermissionSlashInfo Build()
        {
            if (DiscordNetBuilder == null || Permission == null || Handler == null)
            {
                throw new InvalidOperationException("DiscordNetBuilder, Permission and Handler must be set");
            }

            PermissionSlashInfo info =
                new PermissionSlashInfo(DiscordNetBuilder, Handler, Permission, Global, GuildId);

            return info;
        }
    }
}