using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Christofel.Application.Plugins;
using Christofel.BaseLib;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Permissions;
using Christofel.BaseLib.Plugins;
using Christofel.CommandsLib;
using Christofel.CommandsLib.Commands;
using Christofel.CommandsLib.Extensions;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.Application.Commands
{
    public class PluginCommands : CommandHandler
    {
        private readonly PluginService _plugins;
        private readonly IChristofelState _state;
        private readonly ILogger<PluginCommands> _logger;
        private readonly BotOptions _options;
        
        // Plugin command
        // attach, detach, reattach, list subcommands
        public PluginCommands(
            DiscordSocketClient client,
            IPermissionService permissions,
            IChristofelState state,
            PluginService plugins,
            IOptions<BotOptions> options,
            ILogger<PluginCommands> logger
            )
            : base(client, permissions)
        {
            _state = state;
            _plugins = plugins;
            _logger = logger;
            _options = options.Value;
        }

        public override async Task SetupCommandsAsync()
        {
            SlashCommandBuilder pluginBuilder = new SlashCommandBuilderInfo()
                .WithName("plugin")
                .WithDescription("Control attached plugins")
                .WithPermission("application.modules.control")
                .WithGuild(_options.GuildId)
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
                )
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("check")
                    .WithDescription("Checks memory for any detached modules still left")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("list")
                    .WithDescription("Print list of attached plugins")
                    .WithType(ApplicationCommandOptionType.SubCommand));

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
                case "check":
                    await Check(command);
                    break;
            }
        }

        private string GetPluginName(SocketSlashCommand command)
        {
            return (string)command.Data.Options.First().Options.First().Value;
        }
        
        private async Task HandleAttach(SocketSlashCommand command)
        {
            string pluginName = GetPluginName(command);
            
            _logger.LogDebug("Handling command /plugin attach");

            bool attach = true;
            if (!_plugins.Exists(pluginName))
            {
                attach = false;
                await command.RespondAsync("The plugin was not found", ephemeral: true);
            }

            if (_plugins.IsAttached(pluginName))
            {
                attach = false;
                await command.RespondAsync(
                    "Plugin with the same name is already attached. Did you mean to reattach it?",
                    ephemeral: true);
            }

            if (attach)
            {
                try
                {
                    IHasPluginInfo plugin = await _plugins.AttachAsync(_state, pluginName);
                    await command.RespondAsync(
                        $@"Plugin {plugin} was attached", ephemeral: true);
                }
                catch (Exception e)
                {
                    await command.RespondAsync("There was an error. Check the log.", ephemeral: true);
                    throw;
                }
            }
        }
        
        private async Task HandleDetach(SocketSlashCommand command)
        {
            _logger.LogDebug("Handling command /module detach");
            
            string pluginName = GetPluginName(command);

            bool detach = true;
            if (!_plugins.IsAttached(pluginName))
            {
                detach = false;
                await command.RespondAsync(
                    "Could not find attached plugin with this name",
                    ephemeral: true
                );
            }
            
            if (detach)
            {
                IHasPluginInfo plugin = await _plugins.DetachAsync(pluginName);
                await command.RespondAsync(
                    $@"Plugin {plugin} was detached", ephemeral: true);
            }
        }
        
        private async Task HandleReattach(SocketSlashCommand command)
        {
            string pluginName = GetPluginName(command);

            bool reattach = true;
            if (!_plugins.IsAttached(pluginName))
            {
                reattach = false;
                await command.RespondAsync(
                    "Could not find attached plugin with this name",
                    ephemeral: true
                );
            }

            if (reattach)
            {
                IHasPluginInfo plugin = await _plugins.ReattachAsync(_state, pluginName);
                await command.RespondAsync(
                    $@"Plugin {plugin} was detached", ephemeral: true);
            }
        }
        
        private Task HandleList(SocketSlashCommand command)
        {
            IEnumerable<string> plugins = _plugins.AttachedPlugins.Select(x => $@"**{x.Name}** ({x.Version}) - {x.Description}");
            string pluginMessage = "List of attached plugins:\n  " + string.Join("\n  ", plugins);
            
            return command.RespondAsync(pluginMessage, ephemeral: true);
        }

        private Task Check(SocketSlashCommand command)
        {
            _plugins.CheckDetached();
            return command.RespondAsync("Check log for result");
        }
    }
}