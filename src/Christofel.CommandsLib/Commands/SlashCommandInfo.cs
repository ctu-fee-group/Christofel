using System;
using System.Threading.Tasks;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Enums;
using Christofel.BaseLib.Permissions;
using Christofel.CommandsLib.Extensions;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace Christofel.CommandsLib.Commands
{
    public delegate Task SlashCommandHandler(SocketSlashCommand command);
    
    public class SlashCommandInfo
    {
        public static SlashCommandBuilder CreateStatefulBuilder()
        {
            return new SlashCommandBuilderInfo();
        }
        
        public SlashCommandInfo(SlashCommandBuilder builder, string permission,
            SlashCommandHandler handler)
        {
            Builder = builder;
            Handler = handler;
            Permission = new CommandPermission(builder, permission);
        }

        public bool Global { get; set; } = false;
        
        public ulong? GuildId { get; set; }
        
        public SlashCommandHandler Handler { get; }
        
        public SlashCommandBuilder Builder { get; }
        public SlashCommandCreationProperties? BuiltCommand { get; private set; }
        
        public RestApplicationCommand? Command { get; private set; }
        
        public CommandPermission Permission { get; }

        public SlashCommandCreationProperties Build()
        {
            if (BuiltCommand == null)
            {
                BuiltCommand = Builder.Build();
            }

            return BuiltCommand;
        }
        
        public Task<bool> IsForEveryoneAsync(IPermissionsResolver resolver)
        {
            return resolver.HasPermissionAsync(Permission, DiscordTarget.Everyone);
        }

        public Task RefreshCommandAndPermissions()
        {
            throw new NotImplementedException();
        }

        public async Task<IApplicationCommand> RegisterCommandAsync(DiscordRestClient client, IPermissionsResolver resolver)
        {
            if (Command == null)
            {
                SlashCommandCreationProperties command = await SetDefaultPermissionAsync(Build(), resolver);
                
                if (Global)
                {
                    Command = await client.CreateGlobalCommand(command);
                }
                else
                {
                    if (GuildId == null)
                    {
                        throw new ArgumentException("GuildId cannot be null for guild commands");
                    }
                    
                    Command = await client.CreateGuildCommand(command, (ulong)GuildId);
                }
            }

            return Command;
        }

        public async Task RegisterPermissionsAsync(DiscordRestClient client, IPermissionsResolver resolver)
        {
            await RegisterCommandAsync(client, resolver);
            
            if (Command is RestGlobalCommand)
            {
                return; // Global commands cannot have permissions (at least not in Discord.NET yet)
            }
            
            if (Command is RestGuildCommand guildCommand)
            {
                await guildCommand.ModifyCommandPermissions(await resolver.GetSlashCommandPermissionsAsync(Permission));
            }
        }

        private async Task<SlashCommandCreationProperties> SetDefaultPermissionAsync(SlashCommandCreationProperties command, IPermissionsResolver resolver)
        {
            command.DefaultPermission = await IsForEveryoneAsync(resolver);
            return command;
        }
    }
}