using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Christofel.BaseLib.Permissions;
using Christofel.BaseLib.Plugins;
using Christofel.CommandsLib.Commands;
using Christofel.CommandsLib.Extensions;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

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

        public async Task RefreshAsync()
        {
            foreach (SlashCommandInfo command in _commands)
            {
                await command.RefreshCommandAndPermissionsAsync(_permissions.Resolver);
            }
        }

        protected async Task<SlashCommandInfo> RegisterCommandAsync(SlashCommandBuilder builder)
        {
            SlashCommandInfo info = builder.BuildAndGetInfo();
            await info.RegisterCommandAsync(_client.Rest, _permissions.Resolver);
            await info.RegisterPermissionsAsync(_client.Rest, _permissions);

            _commands.Add(info);
            return info;
        }

        protected virtual async Task UnregisterCommandsAsync()
        {
            foreach (SlashCommandInfo info in _commands)
            {
                await info.UnregisterCommand(_permissions);
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
                string name = command.Data.Name;
                SlashCommandInfo? info = _commands.FirstOrDefault(x => x.Builder.Name == name);

                return info?.Handler(command) ?? Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            return UnregisterCommandsAsync();
        }

        public async Task StartAsync()
        {
            await SetupCommandsAsync();
            SetupEvents();
        }
    }
}