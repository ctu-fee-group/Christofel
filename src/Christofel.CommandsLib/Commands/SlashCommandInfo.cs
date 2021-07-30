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

        public async Task RefreshCommandAndPermissionsAsync(IPermissionsResolver resolver)
        {
            if (BuiltCommand == null)
            {
                throw new InvalidOperationException("Cannot refresh without the command built");
            }

            if (Command == null)
            {
                throw new InvalidOperationException("Cannot refresh without the command registered");
            }
            
            await SetDefaultPermissionAsync(BuiltCommand, resolver);
            await RefreshPermissions(resolver);
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

        public async Task UnregisterCommand(IPermissionService permissions)
        {
            permissions.UnregisterPermission(Permission);

            if (Command != null)
            {
                await Command.DeleteAsync();
            }
        }

        public async Task RegisterPermissionsAsync(DiscordRestClient client, IPermissionService permissions)
        {
            IPermissionsResolver resolver = permissions.Resolver;
            
            permissions.RegisterPermission(Permission);
            await RegisterCommandAsync(client, resolver);

            await RefreshPermissions(resolver);
        }
        
        private async Task ModifyCommand(IPermissionsResolver resolver)
        {
            if (Command is RestGlobalCommand globalCommand)
            {
                await globalCommand.ModifyAsync(props => ModifyDefaultPermissionAsync(props, resolver).GetAwaiter().GetResult());
            }
            else if (Command is RestGuildCommand guildCommand)
            {
                await guildCommand.ModifyCommandPermissions(await resolver.GetSlashCommandPermissionsAsync(Permission));
            }
        }

        private async Task RefreshPermissions(IPermissionsResolver resolver)
        {
            if (Command is RestGlobalCommand)
            {
                return; // Global commands cannot have permissions (at least not in Discord.NET yet)
            }
            else if (Command is RestGuildCommand guildCommand)
            {
                ApplicationCommandPermission[] permissions = await resolver.GetSlashCommandPermissionsAsync(Permission);
                if (permissions.Length > 0)
                {
                    // Temporary until Discord.NET labs bug is fixed on nuget (waiting for new release, PR already merged)
                    //await guildCommand.ModifyCommandPermissions(permissions);
                }
            }
        }
        
        private async Task<SlashCommandCreationProperties> SetDefaultPermissionAsync(SlashCommandCreationProperties command, IPermissionsResolver resolver)
        {
            // Temporary until Discord.NET labs bug is fixed on nuget (waiting for new release, PR already merged)
            //command.DefaultPermission = await IsForEveryoneAsync(resolver);
            command.DefaultPermission = true;
            return command;
        }

        private async Task<ApplicationCommandProperties> ModifyDefaultPermissionAsync(ApplicationCommandProperties command, IPermissionsResolver resolver)
        {
            //command.DefaultPermission = await IsForEveryoneAsync(resolver);
            return command;
        }
    }
}