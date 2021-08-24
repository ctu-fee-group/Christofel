using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Application.Plugins;
using Christofel.BaseLib;
using Christofel.BaseLib.Plugins;
using Christofel.CommandsLib;
using Microsoft.Extensions.Logging;

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
        private readonly PluginStorage _storage;
        private readonly ILogger<PluginCommands> _logger;
        private readonly ICommandPermissionsResolver<PermissionSlashInfo> _resolver;

        public PluginCommands(
            ICommandPermissionsResolver<PermissionSlashInfo> resolver,
            IChristofelState state,
            PluginService plugins,
            PluginStorage storage,
            ILogger<PluginCommands> logger
        )
        {
            _resolver = resolver;
            _storage = storage;
            _state = state;
            _plugins = plugins;
            _logger = logger;
        }


        /// <summary>
        /// Handle /plugin attach
        /// </summary>
        /// <param name="command"></param>
        /// <param name="token"></param>
        private async Task HandleAttach(SocketInteraction command, string pluginName,
            CancellationToken token = new CancellationToken())
        {
            _logger.LogDebug("Handling command /plugin attach");

            bool attach = true;
            if (!_plugins.Exists(pluginName))
            {
                attach = false;
                await command.FollowupChunkAsync(
                    "The plugin was not found",
                    ephemeral: true,
                    options: new RequestOptions() {CancelToken = token});
            }

            if (_plugins.IsAttached(pluginName))
            {
                attach = false;
                await command.FollowupChunkAsync(
                    "Plugin with the same name is already attached. Did you mean to reattach it?",
                    ephemeral: true,
                    options: new RequestOptions() {CancelToken = token});
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
                        options: new RequestOptions() {CancelToken = token});
                }
                catch (Exception e)
                {
                    _logger.LogError(0, e, "Error loading plugin");
                    await command.FollowupChunkAsync(
                        "There was an error. Check the log.",
                        ephemeral: true,
                        options: new RequestOptions() {CancelToken = token});
                }
            }
        }

        /// <summary>
        /// Handle /plugin detach
        /// </summary>
        /// <param name="command"></param>
        /// <param name="token"></param>
        private async Task HandleDetach(SocketInteraction command, string pluginName,
            CancellationToken token = new CancellationToken())
        {
            _logger.LogDebug("Handling command /module detach");

            bool detach = true;
            if (!_plugins.IsAttached(pluginName))
            {
                detach = false;
                await command.FollowupChunkAsync(
                    "Could not find attached plugin with this name",
                    ephemeral: true,
                    options: new RequestOptions() {CancelToken = token}
                );
            }

            if (detach)
            {
                IHasPluginInfo plugin = await _plugins.DetachAsync(pluginName, token);
                await command.FollowupChunkAsync(
                    $@"Plugin {plugin} was detached",
                    ephemeral: true,
                    options: new RequestOptions() {CancelToken = token}
                );
            }
        }

        /// <summary>
        /// Handle /plugin reattach
        /// </summary>
        /// <param name="command"></param>
        /// <param name="token"></param>
        private async Task HandleReattach(SocketInteraction command, string pluginName,
            CancellationToken token = new CancellationToken())
        {
            bool reattach = true;
            if (!_plugins.IsAttached(pluginName))
            {
                reattach = false;
                await command.FollowupChunkAsync(
                    "Could not find attached plugin with this name",
                    ephemeral: true,
                    options: new RequestOptions() {CancelToken = token}
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
                        options: new RequestOptions() {CancelToken = token});
                }
                catch (Exception e)
                {
                    _logger.LogError(0, e, "Error reattaching plugin");
                    await command.FollowupChunkAsync(
                        "There was an error. Check the log.",
                        ephemeral: true,
                        options: new RequestOptions() {CancelToken = token});
                }
            }
        }

        /// <summary>
        /// Handle /plugin list
        /// </summary>
        /// <param name="command"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private Task HandleList(SocketInteraction command, CancellationToken token = new CancellationToken())
        {
            IEnumerable<string> plugins = _storage.AttachedPlugins
                .Select(x => $@"**{x.Name}** ({x.Version}) - {x.Description}");
            string pluginMessage = "List of attached plugins:\n  " + string.Join("\n  ", plugins);

            return command.FollowupChunkAsync(
                pluginMessage,
                ephemeral: true,
                options: new RequestOptions() {CancelToken = token});
        }

        /// <summary>
        /// Handle /plugin check
        /// </summary>
        /// <param name="command"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private Task HandleCheck(SocketInteraction command, CancellationToken token = new CancellationToken())
        {
            _plugins.CheckDetached();
            return command.FollowupChunkAsync(
                "Check log for result",
                ephemeral: true,
                options: new RequestOptions() {CancelToken = token});
        }
        
        public SlashCommandOptionBuilder GetAttachSubcommandBuilder()
        {
            return new SlashCommandOptionBuilder()
                    .WithName("attach")
                    .WithDescription("Attach a plugin that is not yet attached")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("pluginname")
                        .WithRequired(true)
                        .WithDescription("Name of the module")
                        .WithType(ApplicationCommandOptionType.String)
                    );
        }
        
        public SlashCommandOptionBuilder GetDetachSubcommandBuilder()
        {
            return new SlashCommandOptionBuilder()
                    .WithName("detach")
                    .WithDescription("Detach a plugin that is already attached")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("pluginname")
                        .WithRequired(true)
                        .WithDescription("Name of the module")
                        .WithType(ApplicationCommandOptionType.String)
                    );
        }
        
        public SlashCommandOptionBuilder GetReattachSubcommandBuilder()
        {
            return new SlashCommandOptionBuilder()
                    .WithName("reattach")
                    .WithDescription("Reattach a plugin that is attached")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("pluginname")
                        .WithRequired(true)
                        .WithDescription("Name of the module")
                        .WithType(ApplicationCommandOptionType.String)
                    );
        }
        
        public SlashCommandOptionBuilder GetCheckSubcommandBuilder()
        {
            return new SlashCommandOptionBuilder()
                    .WithName("check")
                    .WithDescription("Checks memory for any detached modules still left")
                    .WithType(ApplicationCommandOptionType.SubCommand);
        }
        
        public SlashCommandOptionBuilder GetListSubcommandBuilder()
        {
            return new SlashCommandOptionBuilder()
                .WithName("list")
                .WithDescription("Print list of attached plugins")
                .WithType(ApplicationCommandOptionType.SubCommand);
        }
        
        public Task SetupCommandsAsync(IInteractionHolder holder, CancellationToken token = new CancellationToken())
        {
            SubCommandHandlerCreator creator = new SubCommandHandlerCreator();
            DiscordInteractionHandler handler = creator.CreateHandlerForCommand(
                ("attach", (CommandDelegate<string>) HandleAttach),
                ("detach", (CommandDelegate<string>) HandleDetach),
                ("reattach", (CommandDelegate<string>) HandleReattach),
                ("list", (CommandDelegate) HandleList),
                ("check", (CommandDelegate) HandleCheck));

            PermissionSlashInfoBuilder pluginBuilder = new PermissionSlashInfoBuilder()
                .WithPermission("application.plugins.control")
                .WithHandler(handler)
                .WithBuilder(new SlashCommandBuilder()
                    .WithName("plugin")
                    .WithDescription("Control attached plugins")
                    .AddOption(GetAttachSubcommandBuilder())
                    .AddOption(GetDetachSubcommandBuilder())
                    .AddOption(GetReattachSubcommandBuilder())
                    .AddOption(GetCheckSubcommandBuilder())
                    .AddOption(GetListSubcommandBuilder()));

            IInteractionExecutor executor = new InteractionExecutorBuilder<PermissionSlashInfo>()
                .WithLogger(_logger)
                .WithPermissionCheck(_resolver)
                .WithDeferMessage()
                .WithThreadPool()
                .Build();

            holder.AddInteraction(pluginBuilder.Build(), executor);
            return Task.CompletedTask;
        }
    }
}