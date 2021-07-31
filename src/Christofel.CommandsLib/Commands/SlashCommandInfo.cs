using System;
using System.Threading;
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
    public delegate Task SlashCommandHandler(SocketSlashCommand command, CancellationToken token = new CancellationToken());
    
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
        
        public bool Registered { get; set; }

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

        public Task<bool> HasPermissionAsync(SocketUser user, IPermissionsResolver resolver, CancellationToken cancellationToken = new CancellationToken())
        {
            List<DiscordTarget> targets = new List<DiscordTarget>();
            
            if (user is SocketGuildUser guildUser)
            {
                targets.AddRange(guildUser.Roles.Select(x => x.ToDiscordTarget()));
            }
            
            targets.Add(user.ToDiscordTarget());

            return resolver.AnyHasPermissionAsync(Permission, targets, cancellationToken);
        }
        
        public Task<bool> IsForEveryoneAsync(IPermissionsResolver resolver, CancellationToken token = new CancellationToken())
        {
            return resolver.HasPermissionAsync(Permission, DiscordTarget.Everyone, token);
        }

        public async Task RefreshCommandAndPermissionsAsync(IPermissionsResolver resolver, CancellationToken token = new CancellationToken())
        {
            if (BuiltCommand == null)
            {
                throw new InvalidOperationException("Cannot refresh without the command built");
            }

            if (Command == null)
            {
                throw new InvalidOperationException("Cannot refresh without the command registered");
            }
            
            await SetDefaultPermissionAsync(BuiltCommand, resolver, token);
            await RefreshPermissions(resolver, token);
        }

        public async Task<IApplicationCommand> RegisterCommandAsync(DiscordRestClient client, IPermissionsResolver resolver, CancellationToken token = new CancellationToken())
        {
            if (Command == null)
            {
                SlashCommandCreationProperties command = await SetDefaultPermissionAsync(Build(), resolver, token);
                
                if (Global)
                {
                    Command = await client.CreateGlobalCommand(command, new RequestOptions()
                    {
                        CancelToken = token
                    });
                }
                else
                {
                    if (GuildId == null)
                    {
                        throw new ArgumentException("GuildId cannot be null for guild commands");
                    }
                    
                    Command = await client.CreateGuildCommand(command, (ulong)GuildId, new RequestOptions()
                    {
                        CancelToken = token
                    });
                }
            }

            Registered = true;
            return Command;
        }

        public async Task UnregisterCommandAsync(IPermissionService permissions, CancellationToken token = new CancellationToken())
        {
            permissions.UnregisterPermission(Permission);

            if (Command != null)
            {
                await Command.DeleteAsync(new ()
                {
                    CancelToken = token
                });
            }
            
            Registered = false;
        }

        public async Task RegisterPermissionsAsync(DiscordRestClient client, IPermissionService permissions, CancellationToken token = new CancellationToken())
        {
            IPermissionsResolver resolver = permissions.Resolver;
            
            permissions.RegisterPermission(Permission);
            await RegisterCommandAsync(client, resolver, token);

            await RefreshPermissions(resolver, token);
        }
        
        private async Task ModifyCommand(IPermissionsResolver resolver, CancellationToken token = new CancellationToken())
        {
            if (Command is RestGlobalCommand globalCommand)
            {
                await globalCommand.ModifyAsync(props => ModifyDefaultPermissionAsync(props, resolver, token).GetAwaiter().GetResult());
            }
            else if (Command is RestGuildCommand guildCommand)
            {
                await guildCommand.ModifyCommandPermissions(await resolver.GetSlashCommandPermissionsAsync(Permission, token));
            }
        }

        private async Task RefreshPermissions(IPermissionsResolver resolver, CancellationToken token = new CancellationToken())
        {
            if (Command is RestGlobalCommand)
            {
                return; // Global commands cannot have permissions (at least not in Discord.NET yet)
            }
            else if (Command is RestGuildCommand guildCommand)
            {
                ApplicationCommandPermission[] permissions = await resolver.GetSlashCommandPermissionsAsync(Permission, token);
                if (permissions.Length > 0)
                {
                    await guildCommand.ModifyCommandPermissions(permissions);
                }
            }
        }
        
        private async Task<SlashCommandCreationProperties> SetDefaultPermissionAsync(SlashCommandCreationProperties command, IPermissionsResolver resolver, CancellationToken token = new CancellationToken())
        {
            command.DefaultPermission = await IsForEveryoneAsync(resolver, token);
            return command;
        }

        private async Task<ApplicationCommandProperties> ModifyDefaultPermissionAsync(ApplicationCommandProperties command, IPermissionsResolver resolver, CancellationToken token = new CancellationToken())
        {
            command.DefaultPermission = await IsForEveryoneAsync(resolver, token);
            return command;
        }
    }
}