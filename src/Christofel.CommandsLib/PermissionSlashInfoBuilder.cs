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
            if (DiscordNetBuilder is null || Permission is null)
            {
                throw new InvalidOperationException("DiscordNetBuilder, Permission and Handler must be set");
            }

            PermissionSlashInfo info;
            if (Handler is not null)
            {
                info =
                    new PermissionSlashInfo(DiscordNetBuilder, Handler, Permission, Global, GuildId);
            }
            else if (InstancedHandler is not null)
            {
                info =
                    new PermissionSlashInfo(DiscordNetBuilder, InstancedHandler, Permission, Global, GuildId);
            }
            else
            {
                throw new InvalidOperationException("At least one of Handler, InstancedHandler muset be set");
            }
            
            return info;
        }
    }
}