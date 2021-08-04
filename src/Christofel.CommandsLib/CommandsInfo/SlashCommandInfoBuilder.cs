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
        /// Builder used to build SlashCommandCreationOptions
        /// </summary>
        public SlashCommandBuilder? DiscordNetBuilder { get; internal set; }

        /// <summary>
        /// Set SlashCommandCreationOptions builder
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public SlashCommandInfoBuilder WithBuilder(SlashCommandBuilder builder)
        {
            DiscordNetBuilder = builder;
            return this;
        }
        
        /// <summary>
        /// Set command handler deleagate to be called when handling the command
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public SlashCommandInfoBuilder WithHandler(SlashCommandHandler handler)
        {
            Handler = handler;
            return this;
        }

        /// <summary>
        /// Set whether the command should be global or guild
        /// </summary>
        /// <param name="global"></param>
        /// <returns></returns>
        public SlashCommandInfoBuilder SetGlobal(bool global = true)
        {
            Global = global;
            return this;
        }

        /// <summary>
        /// Set Christofel permission for the command
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public SlashCommandInfoBuilder WithPermission(string permission)
        {
            Permission = permission;
            return this;
        }
        
        /// <summary>
        /// Set guild for the guild command to be added to, set the command non-global (guild)
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public SlashCommandInfoBuilder WithGuild(ulong guildId)
        {
            GuildId = guildId;
            Global = false;
            return this;
        }

        /// <summary>
        /// Build SlashCommandInfo
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
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