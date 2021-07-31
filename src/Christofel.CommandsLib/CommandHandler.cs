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

        protected bool AutoDefer { get; set; }
        protected string DeferMessage { get; set; }
        protected RunMode RunMode { get; set; }

        public abstract Task SetupCommandsAsync(CancellationToken token = new CancellationToken());

        public async Task RefreshAsync(CancellationToken token = new CancellationToken())
        {
            foreach (SlashCommandInfo command in _commands)
            {
                await command.RefreshCommandAndPermissionsAsync(_permissions.Resolver, token);
            }
        }

        protected async Task<SlashCommandInfo> RegisterCommandAsync(SlashCommandBuilder builder, CancellationToken token = new CancellationToken())
        {
            SlashCommandInfo info = builder.BuildAndGetInfo();
            await info.RegisterCommandAsync(_client.Rest, _permissions.Resolver, token);
            await info.RegisterPermissionsAsync(_client.Rest, _permissions, token);

            _commands.Add(info);
            return info;
        }

        protected virtual async Task UnregisterCommandsAsync(CancellationToken token = new CancellationToken())
        {
            foreach (SlashCommandInfo info in _commands)
            {
                token.ThrowIfCancellationRequested();
                await info.UnregisterCommandAsync(_permissions, token);
            }
            
            _commands.Clear();
        }

        protected virtual void SetupEvents()
        {
            _client.InteractionCreated += HandleInteractionCreated;
        }

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

        public async Task StopAsync(CancellationToken token = new CancellationToken())
        {
            _client.InteractionCreated -= HandleInteractionCreated;

            _stopping = true;
            _commandsTokenSource.Cancel();
            
            token.ThrowIfCancellationRequested();

            await UnregisterCommandsAsync(token);
        }

        public async Task StartAsync(CancellationToken token = new CancellationToken())
        {
            await SetupCommandsAsync(token);
            token.ThrowIfCancellationRequested();
            SetupEvents();
        }
    }
}