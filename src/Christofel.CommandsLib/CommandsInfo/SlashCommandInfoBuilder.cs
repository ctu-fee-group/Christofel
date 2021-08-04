using System;
using Discord;

namespace Christofel.CommandsLib.CommandsInfo
{
    public class SlashCommandInfoBuilder
    {
        /// <summary>
        /// If the command should be registered as global
        /// </summary>
        public bool Global { get; internal set; }

        /// <summary>
        /// Permission name used for non-global commands
        /// </summary>
        public string? Permission { get; internal set; }

        /// <summary>
        /// Where the command should be added to
        /// </summary>
        public ulong? GuildId { get; internal set; }

        /// <summary>
        /// Handler that will be called when the command was executed by a user
        /// </summary>
        public SlashCommandHandler? Handler { get; internal set; }
        
        /// <summary>
        /// 
        /// </summary>
        public SlashCommandBuilder? DiscordNetBuilder { get; internal set; }

        public SlashCommandInfoBuilder WithBuilder(SlashCommandBuilder builder)
        {
            DiscordNetBuilder = builder;
            return this;
        }

        public SlashCommandInfoBuilder WithHandler(SlashCommandHandler handler)
        {
            Handler = handler;
            return this;
        }

        public SlashCommandInfoBuilder SetGlobal(bool global = true)
        {
            Global = global;
            return this;
        }

        public SlashCommandInfoBuilder WithPermission(string permission)
        {
            Permission = permission;
            Global = false;
            return this;
        }
        
        public SlashCommandInfoBuilder WithGuild(ulong guildId)
        {
            GuildId = guildId;
            return this;
        }

        public SlashCommandInfo Build()
        {
            if (DiscordNetBuilder == null || Permission == null || Handler == null)
            {
                throw new InvalidOperationException("DiscordNetBuilder, Permission and Handler must be set");
            }

            SlashCommandInfo info = new SlashCommandInfo(DiscordNetBuilder, Permission, Handler)
            {
                Global = Global,
                GuildId = GuildId
            };

            return info;
        }
    }
}