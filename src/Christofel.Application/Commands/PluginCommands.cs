using System;
using System.Linq;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Permissions;
using Christofel.CommandsLib;
using Christofel.CommandsLib.Commands;
using Christofel.CommandsLib.Extensions;
using Discord;
using Discord.WebSocket;

namespace Christofel.Application.Commands
{
    public class PluginCommands : CommandHandler
    {
        private IReadableConfig _config;
        
        // Plugin command
        // attach, detach, reattach, list subcommands
        public PluginCommands(DiscordSocketClient client, IPermissionService permissions, IReadableConfig config)
            : base(client, permissions)
        {
            _config = config;
        }

        public override async Task SetupCommandsAsync()
        {
            SlashCommandBuilder pluginBuilder = new SlashCommandBuilderInfo()
                .WithName("plugin")
                .WithDescription("Control attached plugins")
                .WithPermission("application.modules.control")
                .WithGuild(await _config.GetAsync<ulong>("discord.bot.guild"))
                .WithHandler(HandlePluginCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("attach")
                    .WithDescription("Attach a plugin that is not yet attached")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("name")
                        .WithDescription("Name of the module")
                        .WithType(ApplicationCommandOptionType.String)
                    )
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("detach")
                    .WithDescription("Detach a plugin that is already attached")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("name")
                        .WithDescription("Name of the module")
                        .WithType(ApplicationCommandOptionType.String)
                    )
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("reattach")
                    .WithDescription("Reattach a plugin that is attached")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("name")
                        .WithDescription("Name of the module")
                        .WithType(ApplicationCommandOptionType.String)
                    )
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("list")
                    .WithDescription("Print list of attached plugins"));

            await RegisterCommandAsync(pluginBuilder);
        }

        private async Task HandlePluginCommand(SocketSlashCommand command)
        {
            string subcommand = command.Data.Options.First().Name;

            switch (subcommand)
            {
                case "attach":
                    await HandleAttach(command);
                    break;
                case "reattach":
                    await HandleReattach(command);
                    break;
                case "detach":
                    await HandleDetach(command);
                    break;
                case "list":
                    await HandleList(command);
                    break;
            }
        }

        private string GetModuleName(SocketSlashCommand command)
        {
            return (string)command.Data.Options.First().Options.First().Value;
        }
        
        private Task HandleAttach(SocketSlashCommand command)
        {
            string module = GetModuleName(command);
            return Task.CompletedTask;
        }
        
        private Task HandleDetach(SocketSlashCommand command)
        {
            string module = GetModuleName(command);
            return Task.CompletedTask;
        }
        
        private Task HandleReattach(SocketSlashCommand command)
        {
            string module = GetModuleName(command);
            return Task.CompletedTask;
        }
        
        private Task HandleList(SocketSlashCommand command)
        {
            string module = GetModuleName(command);
            return Task.CompletedTask;
        }
    }
}