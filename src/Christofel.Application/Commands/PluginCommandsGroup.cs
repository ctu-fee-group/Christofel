using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Application.Plugins;
using Christofel.BaseLib;
using Christofel.BaseLib.Plugins;
using Christofel.CommandsLib;
using Microsoft.Extensions.Logging;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Feedback.Services;

namespace Christofel.Application.Commands
{
    /// <summary>
    /// Handler of /plugin attach, detach, reattach, list, check commands
    /// </summary>
    [Group("plugins")]
    [DiscordDefaultPermission(false)]
    [RequirePermission("application.plugins")]
    public class PluginCommands : CommandGroup
    {
        private readonly PluginService _plugins;
        private readonly IChristofelState _state;
        private readonly PluginStorage _storage;
        private readonly ILogger<PluginCommands> _logger;
        private readonly FeedbackService _feedbackService;

        public PluginCommands(
            IChristofelState state,
            PluginService plugins,
            PluginStorage storage,
            ILogger<PluginCommands> logger,
            FeedbackService feedbackService
        )
        {
            _feedbackService = feedbackService;
            _storage = storage;
            _state = state;
            _plugins = plugins;
            _logger = logger;
        }

        [Command("attach")]
        [Description("Attach given plugin by name")]
        private async Task HandleAttach([Description("Name of the plugin to attach")] string pluginName)
        {
            _logger.LogDebug("Handling command /plugin attach");

            bool attach = true;
            if (!_plugins.Exists(pluginName))
            {
                attach = false;
                await _feedbackService.SendContextualErrorAsync("The plugin was not found", ct: CancellationToken);
            }

            if (_plugins.IsAttached(pluginName))
            {
                attach = false;
                await _feedbackService.SendContextualErrorAsync(
                    "Plugin with the same name is already attached. Did you mean to reattach it?",
                    ct: CancellationToken);
            }

            if (attach)
            {
                try
                {
                    IHasPluginInfo plugin = await _plugins.AttachAsync(_state, pluginName, CancellationToken);
                    await _feedbackService.SendContextualSuccessAsync($"Plugin {plugin} was attached",
                        ct: CancellationToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error loading plugin");
                    await _feedbackService.SendContextualErrorAsync(
                        "There was an error. Check the log",
                        ct: CancellationToken);
                }
            }
        }

        [Command("detach")]
        [Description("Detach given plugin by name")]
        private async Task HandleDetach([Description("Name of the plugin to detach")] string pluginName)
        {
            _logger.LogDebug("Handling command /module detach");

            bool detach = true;
            if (!_plugins.IsAttached(pluginName))
            {
                detach = false;
                await _feedbackService.SendContextualErrorAsync("Could not find attached plugin with this name",
                    ct: CancellationToken);
            }

            if (detach)
            {
                try
                {
                    IHasPluginInfo plugin = await _plugins.DetachAsync(pluginName, CancellationToken);
                    await _feedbackService.SendContextualSuccessAsync($"Plugin {plugin} was detached",
                        ct: CancellationToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error detaching a plugin");
                    await _feedbackService.SendContextualErrorAsync(
                        "There was an error. Check the log",
                        ct: CancellationToken);
                }
            }
        }

        [Command("reattach")]
        [Description("Reattach given plugin by name")]
        private async Task HandleReattach([Description("Name of the plugin to reattach")] string pluginName)
        {
            bool reattach = true;
            if (!_plugins.IsAttached(pluginName))
            {
                reattach = false;
                await _feedbackService.SendContextualErrorAsync("Could not find attached plugin with this name",
                    ct: CancellationToken);
            }

            if (reattach)
            {
                try
                {
                    IHasPluginInfo plugin = await _plugins.ReattachAsync(_state, pluginName, CancellationToken);
                    await _feedbackService.SendContextualSuccessAsync($"Plugin {plugin} was reattached",
                        ct: CancellationToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(0, e, "Error reattaching plugin");
                    await _feedbackService.SendContextualErrorAsync(
                        "There was an error. Check the log",
                        ct: CancellationToken);
                }
            }
        }
        
        [Command("list")]
        [Description("List all attached plugins")]
        private Task HandleList()
        {
            IEnumerable<string> plugins = _storage.AttachedPlugins
                .Select(x => $@"**{x.Name}** ({x.Version}) - {x.Description}");
            string pluginMessage = "List of attached plugins:\n  " + string.Join("\n  ", plugins);

            return _feedbackService.SendContextualSuccessAsync(pluginMessage,
                ct: CancellationToken);
        }
        
        [Command("check")]
        [Description("Check if detached plugins freed the memory")]
        private Task HandleCheck()
        {
            _plugins.CheckDetached();
            return _feedbackService.SendContextualInfoAsync("Check log for result",
                ct: CancellationToken);
        }
    }
}