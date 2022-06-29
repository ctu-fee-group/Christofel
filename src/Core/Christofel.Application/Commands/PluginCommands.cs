//
//  PluginCommands.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Christofel.CommandsLib.Permissions;
using Christofel.Plugins;
using Christofel.Plugins.Services;
using Microsoft.Extensions.Logging;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

namespace Christofel.Application.Commands
{
    /// <summary>
    /// Handles /plugins attach, detach, reattach, list, check commands.
    /// </summary>
    [Group("plugins")]
    [RequirePermission("application.plugins")]
    [Ephemeral]
    public class PluginCommands : CommandGroup
    {
        private readonly FeedbackService _feedbackService;
        private readonly ILogger<PluginCommands> _logger;
        private readonly PluginService _plugins;
        private readonly PluginStorage _storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginCommands"/> class.
        /// </summary>
        /// <param name="plugins">The plugin service.</param>
        /// <param name="storage">The plugin storage.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="feedbackService">The feedback service.</param>
        public PluginCommands
        (
            PluginService plugins,
            PluginStorage storage,
            ILogger<PluginCommands> logger,
            FeedbackService feedbackService
        )
        {
            _feedbackService = feedbackService;
            _storage = storage;
            _plugins = plugins;
            _logger = logger;
        }

        /// <summary>
        /// Handles /plugins attach command.
        /// </summary>
        /// <remarks>
        /// Tries to attach and initialized plugin.
        /// </remarks>
        /// <param name="pluginName">The name of the plugin.</param>
        /// <returns>A result that may not have succeeded.</returns>
        [Command("attach")]
        [Description("Attach given plugin by name")]
        [RequirePermission("application.plugins.attach")]
        public async Task<IResult> HandleAttach([Description("Name of the plugin to attach")] string pluginName)
        {
            var attach = true;
            if (!_plugins.Exists(pluginName))
            {
                attach = false;
                await _feedbackService.SendContextualErrorAsync("The plugin was not found", ct: CancellationToken);
            }

            if (_plugins.IsAttached(pluginName))
            {
                attach = false;
                await _feedbackService.SendContextualErrorAsync
                (
                    "Plugin with the same name is already attached. Did you mean to reattach it?",
                    ct: CancellationToken
                );
            }

            if (attach)
            {
                try
                {
                    IHasPluginInfo plugin = await _plugins.AttachAsync(pluginName, CancellationToken);
                    await _feedbackService.SendContextualSuccessAsync
                    (
                        $"Plugin {plugin} was attached",
                        ct: CancellationToken
                    );
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error loading plugin");
                    await _feedbackService.SendContextualErrorAsync
                    (
                        "There was an error. Check the log",
                        ct: CancellationToken
                    );
                }
            }

            return Result.FromSuccess();
        }

        /// <summary>
        /// Handles /plugins detach command.
        /// </summary>
        /// <remarks>
        /// Destroys and detaches plugin.
        /// </remarks>
        /// <param name="pluginName">The name of the plugin.</param>
        /// <returns>A result that may not have succeeded.</returns>
        [Command("detach")]
        [Description("Detach given plugin by name")]
        [RequirePermission("application.plugins.detach")]
        public async Task<IResult> HandleDetach([Description("Name of the plugin to detach")] string pluginName)
        {
            var detach = true;
            if (!_plugins.IsAttached(pluginName))
            {
                detach = false;
                await _feedbackService.SendContextualErrorAsync
                (
                    "Could not find attached plugin with this name",
                    ct: CancellationToken
                );
            }

            if (detach)
            {
                try
                {
                    IHasPluginInfo plugin = await _plugins.DetachAsync(pluginName, CancellationToken);
                    await _feedbackService.SendContextualSuccessAsync
                    (
                        $"Plugin {plugin} was detached",
                        ct: CancellationToken
                    );
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error detaching a plugin");
                    await _feedbackService.SendContextualErrorAsync
                    (
                        "There was an error. Check the log",
                        ct: CancellationToken
                    );
                }
            }

            return Result.FromSuccess();
        }

        /// <summary>
        /// Handles /plugins reattach.
        /// </summary>
        /// <remarks>
        /// Tries to detach and then attach the given plugin.
        /// </remarks>
        /// <param name="pluginName">The name of the plugin.</param>
        /// <returns>A result that may not have succeeded.</returns>
        [Command("reattach")]
        [Description("Reattach given plugin by name")]
        [RequirePermission("application.plugins.reattach")]
        public async Task<IResult> HandleReattach([Description("Name of the plugin to reattach")] string pluginName)
        {
            var reattach = true;
            if (!_plugins.IsAttached(pluginName))
            {
                reattach = false;
                await _feedbackService.SendContextualErrorAsync
                (
                    "Could not find attached plugin with this name",
                    ct: CancellationToken
                );
            }

            if (reattach)
            {
                try
                {
                    IHasPluginInfo plugin = await _plugins.ReattachAsync(pluginName, CancellationToken);
                    await _feedbackService.SendContextualSuccessAsync
                    (
                        $"Plugin {plugin} was reattached",
                        ct: CancellationToken
                    );
                }
                catch (Exception e)
                {
                    _logger.LogError(0, e, "Error reattaching plugin");
                    await _feedbackService.SendContextualErrorAsync
                    (
                        "There was an error. Check the log",
                        ct: CancellationToken
                    );
                }
            }

            return Result.FromSuccess();
        }

        /// <summary>
        /// Handles /plugins list command.
        /// </summary>
        /// <remarks>
        /// Lists attached and attachable plugins.
        /// </remarks>
        /// <returns>A result that may not have succeeded.</returns>
        [Command("list")]
        [Description("List all attached plugins")]
        [RequirePermission("application.plugins.list")]
        public async Task<Result<IReadOnlyList<IMessage>>> HandleList()
        {
            IEnumerable<string> attachedPlugins = _storage.AttachedPlugins
                .Select(x => $@"  - **{x.Name}** ({x.Version}) - _{x.Description}_");
            IEnumerable<string> attachablePlugins = _plugins.GetAttachablePluginNames()
                .Select(x => $"  - **{x}**");

            string pluginMessage = "List of attached plugins:\n" + string.Join("\n", attachedPlugins) + "\n";
            string attachablePluginMessage = "List of attachable plugins:\n" + string.Join("\n", attachablePlugins);

            var result = await _feedbackService.SendContextualSuccessAsync
            (
                pluginMessage,
                ct: CancellationToken
            );

            if (!result.IsSuccess)
            {
                return result;
            }

            return await _feedbackService.SendContextualSuccessAsync
            (
                attachablePluginMessage,
                ct: CancellationToken
            );
        }

        /// <summary>
        /// Handles /plugins check command.
        /// </summary>
        /// <returns>A result that may not have succeeded.</returns>
        [Command("check")]
        [Description("Check if detached plugins freed the memory")]
        [RequirePermission("application.plugins.check")]
        public Task<Result<IReadOnlyList<IMessage>>> HandleCheck()
        {
            _plugins.CheckDetached();
            return _feedbackService.SendContextualInfoAsync
            (
                "Check log for result",
                ct: CancellationToken
            );
        }
    }
}