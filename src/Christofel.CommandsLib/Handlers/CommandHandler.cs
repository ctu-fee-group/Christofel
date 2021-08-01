using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Permissions;
using Christofel.BaseLib.Plugins;
using Christofel.CommandsLib.Commands;
using Christofel.CommandsLib.Extensions;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Christofel.CommandsLib
{
    /// <summary>
    /// Base class for a command handler
    /// </summary>
    /// <remarks>
    /// Exposes basic command handling helper commands
    /// </remarks>
    public abstract class CommandHandler : IStartable, IRefreshable, IStoppable
    {
        protected readonly DiscordSocketClient _client;
        protected readonly List<SlashCommandInfo> _commands;
        protected readonly IPermissionService _permissions;

        protected readonly CancellationTokenSource _commandsTokenSource;
        protected bool _stopping;

        protected readonly ILogger _logger;
        
        public CommandHandler(DiscordSocketClient client, IPermissionService permissions, ILogger logger)
        {
            _logger = logger;
            _commandsTokenSource = new CancellationTokenSource();
            _client = client;
            _permissions = permissions;
            _commands = new List<SlashCommandInfo>();

            AutoDefer = true;
            RunMode = RunMode.NewThread;
            DeferMessage = "I am thinking ...";
        }

        /// <summary>
        /// Specifies if the command should be deferred automatically along with DeferMessage
        /// </summary>
        protected bool AutoDefer { get; set; }
        
        /// <summary>
        /// Specifies the message to send when AutoDefer is true
        /// </summary>
        protected string DeferMessage { get; set; }
        
        /// <summary>
        /// Sets RunMode, <see cref="RunMode"/> for more information
        /// </summary>
        protected RunMode RunMode { get; set; }

        /// <summary>
        /// Builds and registers slash commands
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public abstract Task SetupCommandsAsync(CancellationToken token = new CancellationToken());

        /// <summary>
        /// Refreshes command permissions
        /// </summary>
        /// <param name="token"></param>
        public async Task RefreshAsync(CancellationToken token = new CancellationToken())
        {
            foreach (SlashCommandInfo command in _commands)
            {
                await command.RefreshCommandAndPermissionsAsync(_permissions.Resolver, token);
            }
        }

        /// <summary>
        /// Register command and save it to commands collection
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected async Task<SlashCommandInfo> RegisterCommandAsync(SlashCommandBuilder builder, CancellationToken token = new CancellationToken())
        {
            SlashCommandInfo info = builder.BuildAndGetInfo();
            await info.RegisterCommandAsync(_client.Rest, _permissions.Resolver, token);
            await info.RegisterPermissionsAsync(_client.Rest, _permissions, token);

            _commands.Add(info);
            return info;
        }

        /// <summary>
        /// Unregister all commands that are stored in commands collection
        /// </summary>
        /// <param name="token"></param>
        protected virtual async Task UnregisterCommandsAsync(CancellationToken token = new CancellationToken())
        {
            foreach (SlashCommandInfo info in _commands)
            {
                token.ThrowIfCancellationRequested();
                await info.UnregisterCommandAsync(_permissions, token);
            }
            
            _commands.Clear();
        }

        /// <summary>
        /// Setup events for handling
        /// </summary>
        protected virtual void SetupEvents()
        {
            _client.InteractionCreated += HandleInteractionCreated;
        }

        /// <summary>
        /// Handle InteractionCreated event
        /// </summary>
        /// <remarks>
        /// Calls HandleCommand if the interaction is a SocketSlashCommand and found in commands collection
        /// </remarks>
        /// <param name="arg"></param>
        /// <returns></returns>
        protected virtual Task HandleInteractionCreated(SocketInteraction arg)
        {
            if (arg is SocketSlashCommand command)
            {
                SlashCommandInfo? info = _commands.FirstOrDefault(x => x.Builder.Name == command.Data.Name);

                if (info != null)
                {
                    return HandleCommand(info, command);
                }

                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Handle Interaction SlashCommand
        /// </summary>
        /// <remarks>
        /// 1. Checks if the user has permissions to command
        /// 2. If AutoDefer is true, respond with send DeferMessage
        /// 3. Based on RunMode
        ///   3.1 SameThread handles command on same thread
        ///   3.2 NewThread handles command in new thread using Task.Run
        /// </remarks>
        /// <param name="info"></param>
        /// <param name="command"></param>
        protected virtual async Task HandleCommand(SlashCommandInfo info, SocketSlashCommand command)
        {
            if (!await info.HasPermissionAsync(command.User, _permissions.Resolver))
            {
                return;
            }
            
            if (AutoDefer)
            {
                await command.RespondChunkAsync(DeferMessage, ephemeral: true);
            }
            
            if (RunMode == RunMode.SameThread)
            {
                await info.Handler(command, _commandsTokenSource.Token);
            }
            else
            {
                #pragma warning disable 4014
                Task.Run(async () => 
                #pragma warning restore 4014
                {
                    try
                    {
                        await info.Handler(command, _commandsTokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(0, e, "Command handler has thrown an exception while running in thread");
                    }
                });
            }
        }

        /// <summary>
        /// Stops all current running handlers
        /// and unregister commands
        /// </summary>
        /// <param name="token"></param>
        public async Task StopAsync(CancellationToken token = new CancellationToken())
        {
            _client.InteractionCreated -= HandleInteractionCreated;

            _stopping = true;
            _commandsTokenSource.Cancel();
            
            token.ThrowIfCancellationRequested();

            await UnregisterCommandsAsync(token);
        }

        /// <summary>
        /// Registers commands and events
        /// </summary>
        /// <param name="token"></param>
        public async Task StartAsync(CancellationToken token = new CancellationToken())
        {
            await SetupCommandsAsync(token);
            token.ThrowIfCancellationRequested();
            SetupEvents();
        }
    }
}