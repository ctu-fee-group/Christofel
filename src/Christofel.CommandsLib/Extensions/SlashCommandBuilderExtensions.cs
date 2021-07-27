using System;
using System.Runtime.CompilerServices;
using Discord;

namespace Christofel.CommandsLib.Extensions
{
    public static class SlashCommandBuilderExtensions
    {
        private static Exception WrongType =>
            new ArgumentException("Command builder is not the right type. Should be SlashCommandBuilderInfo"); 
        
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
                info.GuildId = info.GuildId;
                info.Build();

                return info;
            }

            throw WrongType;
        }
        
        public static SlashCommandBuilder SetGlobal(this SlashCommandBuilder builder, bool global = true)
        {
            if (builder is SlashCommandBuilderInfo statefulBuilder)
            {
                statefulBuilder.Global = global;
                return statefulBuilder;
            }

            throw WrongType;
        }
        
        public static SlashCommandBuilder WithHandler(this SlashCommandBuilder builder, SlashCommandHandler handler)
        {
            if (builder is SlashCommandBuilderInfo statefulBuilder)
            {
                statefulBuilder.Handler = handler;
                return statefulBuilder;
            }

            throw WrongType;
        }
        
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