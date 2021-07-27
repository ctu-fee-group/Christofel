using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace Christofel.CommandsLib
{
    public abstract class CommandHandler : IAsyncDisposable, IDisposable
    {
        protected readonly DiscordSocketClient _client;
        protected readonly List<RestApplicationCommand> _commands;
        
        public CommandHandler(DiscordSocketClient client)
        {
            _client = client;
            _commands = new List<RestApplicationCommand>();
        }
        
        public abstract Task SetupCommandsAsync();
        protected abstract Task HandleCommand(SocketSlashCommand command);

        public async Task InitAsync()
        {
            await SetupCommandsAsync();
            SetupEvents();
        }
        
        public async ValueTask DisposeAsync()
        {
            await UnregisterCommandsAsync();
        }
        
        public void Dispose()
        {
            DisposeAsync().GetAwaiter().GetResult();
        }

        protected async Task<RestGlobalCommand> RegisterGlobalCommandAsync(SlashCommandBuilder builder)
        {
            RestGlobalCommand command = await _client.Rest.CreateGlobalCommand(builder.Build());

            _commands.Add(command);
            return command;
        }
        
        protected async Task<RestGuildCommand> RegisterGuildCommandAsync(ulong guildId, SlashCommandBuilder builder)
        {
            RestGuildCommand command = await _client.Rest.CreateGuildCommand(builder.Build(), guildId);

            _commands.Add(command);
            return command;
        }


        protected virtual async Task UnregisterCommandsAsync()
        {
            foreach (RestApplicationCommand command in _commands)
            {
                await command.DeleteAsync();
            }
            
            _commands.Clear();
        }

        protected virtual void SetupEvents()
        {
            _client.InteractionCreated += HandleInteractionCreated;
        }

        protected virtual async Task HandleInteractionCreated(SocketInteraction arg)
        {
            if (arg is SocketSlashCommand command)
            {
                await HandleCommand(command);
            }
        }
    }
}