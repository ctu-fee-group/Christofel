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
        
        public CommandHandler(DiscordSocketClient client, IPermissionService permissions)
        {
            _client = client;
            _permissions = permissions;
            _commands = new List<SlashCommandInfo>();
        }
        
        public abstract Task SetupCommandsAsync();

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
            _client.InteractionCreated -= HandleInteractionCreated;
        }

        protected virtual void SetupEvents()
        {
            _client.InteractionCreated += HandleInteractionCreated;
        }

        protected virtual Task HandleInteractionCreated(SocketInteraction arg)
        {
            if (arg is SocketSlashCommand command)
            {
                string name = command.Data.Name;
                SlashCommandInfo? info = _commands.FirstOrDefault(x => x.Builder.Name == name);

                return info?.Handler(command) ?? Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        public Task StopAsync()

        public async Task StopAsync(CancellationToken token = new CancellationToken())
        {

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