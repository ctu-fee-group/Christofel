using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Application.Plugins;
using Christofel.BaseLib;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Permissions;
using Christofel.BaseLib.Plugins;
using Christofel.CommandsLib;
using Christofel.CommandsLib.Commands;
using Christofel.CommandsLib.CommandsInfo;
using Christofel.CommandsLib.Executors;
using Christofel.CommandsLib.Extensions;
using Christofel.CommandsLib.HandlerCreator;
using Christofel.CommandsLib.Handlers;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.Application.Commands
{
    /// <summary>
    /// Handler of /plugin attach, detach, reattach, list, check commands
    /// </summary>
    public class PluginCommands : ICommandGroup
    {
        private delegate Task PluginDelegate(SocketSlashCommand command, string pluginName, CancellationToken token);
        
        private readonly PluginService _plugins;
        private readonly IChristofelState _state;
        private readonly BotOptions _options;
        private readonly PluginStorage _storage;
        private readonly ILogger<PluginCommands> _logger;
        private readonly IPermissionsResolver _resolver;
        
        public PluginCommands(
            IPermissionsResolver resolver,
            IChristofelState state,
            PluginService plugins,
            IOptions<BotOptions> options,
            PluginStorage storage,
            ILogger<PluginCommands> logger
            )
        {
            _resolver = resolver;
            _storage = storage;
            _state = state;
            _plugins = plugins;
            _options = options.Value;
            _logger = logger;
        }

        public Task SetupCommandsAsync(ICommandHolder holder, CancellationToken token = new CancellationToken())
        {
            ICommandHandlerCreator<string> creator = new SubCommandHandlerCreator();
            SlashCommandHandler handler = creator.CreateHandlerForCommand(
                ("attach", (CommandDelegate<string>)HandleAttach),
                ("detach", (CommandDelegate<string>)HandleDetach),
                ("reattach", (CommandDelegate<string>)HandleReattach),
                ("list", (CommandDelegate)HandleList),
                ("check", (CommandDelegate)HandleCheck));
            
            SlashCommandBuilder pluginBuilder = new SlashCommandBuilderInfo()
                .WithName("plugin")
                .WithDescription("Control attached plugins")
                .WithPermission("application.plugins.control")
                .WithGuild(_options.GuildId)
                .WithHandler(handler)
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

            ICommandExecutor executor = new CommandExecutorBuilder()
                .WithLogger(_logger)
                .WithPermissionsCheck(_resolver)
                .WithDeferMessage()
                .WithThreadPool()
                .Build();
            
            holder.AddCommand(pluginBuilder, executor);
            return Task.CompletedTask;
        }
        

        /// <summary>
        /// Handle /plugin attach
        /// </summary>
        /// <param name="command"></param>
        /// <param name="token"></param>
        private async Task HandleAttach(SocketSlashCommand command, string pluginName, CancellationToken token = new CancellationToken())
        {
            _logger.LogDebug("Handling command /plugin attach");

            bool attach = true;
            if (!_plugins.Exists(pluginName))
            {
                attach = false;
                await command.FollowupChunkAsync(
                    "The plugin was not found",
                    ephemeral: true,
                    options: new RequestOptions() { CancelToken = token });
            }

            if (_plugins.IsAttached(pluginName))
            {
                attach = false;
                await command.FollowupChunkAsync(
                    "Plugin with the same name is already attached. Did you mean to reattach it?",
                    ephemeral: true,
                    options: new RequestOptions() { CancelToken = token });
            }

            token.ThrowIfCancellationRequested();
            if (attach)
            {
                try
                {
                    IHasPluginInfo plugin = await _plugins.AttachAsync(_state, pluginName, token);
                    await command.FollowupChunkAsync(
                        $@"Plugin {plugin} was attached",
                        ephemeral: true,
                        options: new RequestOptions() { CancelToken = token });
                }
                catch (Exception e)
                {
                    _logger.LogError(0, e, "Error loading plugin");
                    await command.FollowupChunkAsync(
                        "There was an error. Check the log.",
                        ephemeral: true,
                        options: new RequestOptions() { CancelToken = token });
                }
            }
        }
        
        /// <summary>
        /// Handle /plugin detach
        /// </summary>
        /// <param name="command"></param>
        /// <param name="token"></param>
        private async Task HandleDetach(SocketSlashCommand command, string pluginName, CancellationToken token = new CancellationToken())
        {
            _logger.LogDebug("Handling command /module detach");
            
            bool detach = true;
            if (!_plugins.IsAttached(pluginName))
            {
                detach = false;
                await command.FollowupChunkAsync(
                    "Could not find attached plugin with this name",
                    ephemeral: true,
                    options: new RequestOptions() { CancelToken = token }
                );
            }
            
            if (detach)
            {
                IHasPluginInfo plugin = await _plugins.DetachAsync(pluginName, token);
                await command.FollowupChunkAsync(
                    $@"Plugin {plugin} was detached",
                    ephemeral: true,
                    options: new RequestOptions() { CancelToken = token }
                    );
            }
        }
        
        /// <summary>
        /// Handle /plugin reattach
        /// </summary>
        /// <param name="command"></param>
        /// <param name="token"></param>
        private async Task HandleReattach(SocketSlashCommand command, string pluginName, CancellationToken token = new CancellationToken())
        {
            bool reattach = true;
            if (!_plugins.IsAttached(pluginName))
            {
                reattach = false;
                await command.FollowupChunkAsync(
                    "Could not find attached plugin with this name",
                    ephemeral: true,
                    options: new RequestOptions() { CancelToken = token }
                );
            }

            if (reattach)
            {
                try
                {
                    IHasPluginInfo plugin = await _plugins.ReattachAsync(_state, pluginName, token);
                    await command.FollowupChunkAsync(
                        $@"Plugin {plugin} was reattached",
                        ephemeral: true,
                        options: new RequestOptions() { CancelToken = token });
                }
                catch (Exception e)
                {
                    _logger.LogError(0, e, "Error reattaching plugin");
                    await command.FollowupChunkAsync(
                        "There was an error. Check the log.",
                        ephemeral: true,
                        options: new RequestOptions() { CancelToken = token });
                }
            }
        }
        
        /// <summary>
        /// Handle /plugin list
        /// </summary>
        /// <param name="command"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private Task HandleList(SocketSlashCommand command, CancellationToken token = new CancellationToken())
        {
            IEnumerable<string> plugins = _storage.AttachedPlugins
                .Select(x => $@"**{x.Name}** ({x.Version}) - {x.Description}");
            string pluginMessage = "List of attached plugins:\n  " + string.Join("\n  ", plugins);
            
            return command.FollowupChunkAsync(
                pluginMessage,
                ephemeral: true,
                options: new RequestOptions() { CancelToken = token });
        }

        /// <summary>
        /// Handle /plugin check
        /// </summary>
        /// <param name="command"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private Task HandleCheck(SocketSlashCommand command, CancellationToken token = new CancellationToken())
        {
            _plugins.CheckDetached();
            return command.FollowupChunkAsync(
                "Check log for result",
                ephemeral: true,
                options: new RequestOptions() { CancelToken = token });
        }
    }
}
