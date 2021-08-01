using System;
using System.Runtime.CompilerServices;
using Christofel.CommandsLib.CommandsInfo;
using Discord;

namespace Christofel.CommandsLib.Extensions
{
    public static class SlashCommandBuilderExtensions
    {
        private static Exception WrongType =>
            new ArgumentException("Command builder is not the right type. Should be SlashCommandBuilderInfo"); 
        
        /// <summary>
        /// Builds SlashCommand and returns SlashCommandInfo
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        public static SlashCommandInfo BuildAndGetInfo(this SlashCommandBuilder builder)
        {
            if (builder is SlashCommandBuilderInfo statefulBuilder)
            {
                if (statefulBuilder.Handler == null || statefulBuilder.Permission == null)
                {
                    throw new InvalidOperationException("Handler or Permission in CommandBuilder is null.");
                }

                SlashCommandInfo info = new SlashCommandInfo(builder, statefulBuilder.Permission, statefulBuilder.Handler);
                info.Global = statefulBuilder.Global;
                info.GuildId = statefulBuilder.GuildId;
                info.Build();

                return info;
            }

            throw WrongType;
        }
        
        /// <summary>
        /// Sets Global attribute on the command
        /// </summary>
        /// <remarks>
        /// Sets whether the command should be global or guild only
        /// </remarks>
        /// <param name="builder"></param>
        /// <param name="global"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static SlashCommandBuilder SetGlobal(this SlashCommandBuilder builder, bool global = true)
        {
            if (builder is SlashCommandBuilderInfo statefulBuilder)
            {
                statefulBuilder.Global = global;
                return statefulBuilder;
            }

            throw WrongType;
        }
        
        /// <summary>
        /// Registers handler to handle execution of command
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static SlashCommandBuilder WithHandler(this SlashCommandBuilder builder, SlashCommandHandler handler)
        {
            if (builder is SlashCommandBuilderInfo statefulBuilder)
            {
                statefulBuilder.Handler = handler;
                return statefulBuilder;
            }

            throw WrongType;
        }
        
        /// <summary>
        /// Sets guild of the command and sets global to false
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static SlashCommandBuilder WithGuild(this SlashCommandBuilder builder, ulong guildId)
        {
            if (builder is SlashCommandBuilderInfo statefulBuilder)
            {
                statefulBuilder.Global = false;
                statefulBuilder.GuildId = guildId;
                return statefulBuilder;
            }

            throw WrongType;
        }
        
        /// <summary>
        /// Sets permission name for the command
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="permission"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static SlashCommandBuilder WithPermission(this SlashCommandBuilder builder, string permission)
        {
            if (builder is SlashCommandBuilderInfo statefulBuilder)
            {
                statefulBuilder.Permission = permission;
                return statefulBuilder;
            }

            throw WrongType;
        }
    }
}