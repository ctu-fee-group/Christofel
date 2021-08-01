using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Enums;
using Christofel.BaseLib.Extensions;
using Christofel.BaseLib.Permissions;
using Christofel.CommandsLib.Extensions;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace Christofel.CommandsLib.Commands
{
    public delegate Task SlashCommandHandler(SocketSlashCommand command, CancellationToken token = new CancellationToken());
    
    /// <summary>
    /// Information about a slash command
    /// </summary>
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
        
        /// <summary>
        /// If the command was registered yet
        /// </summary>
        public bool Registered { get; set; }

        /// <summary>
        /// Whether it should be a global command
        /// </summary>
        public bool Global { get; set; } = false;
        
        /// <summary>
        /// What guild to add the command to
        /// </summary>
        public ulong? GuildId { get; set; }
        
        /// <summary>
        /// What handler to execute when command execution is requested
        /// </summary>
        public SlashCommandHandler Handler { get; }
        
        /// <summary>
        /// The builder that will be used to build the command
        /// </summary>
        public SlashCommandBuilder Builder { get; }
        
        /// <summary>
        /// Built command that is set after calling Build()
        /// </summary>
        public SlashCommandCreationProperties? BuiltCommand { get; private set; }
        
        /// <summary>
        /// Registered command that is set after calling RegisterCommandAsync
        /// </summary>
        public RestApplicationCommand? Command { get; private set; }
        
        /// <summary>
        /// Permission of the command
        /// </summary>
        public CommandPermission Permission { get; }

        /// <summary>
        /// Build the command creation properties
        /// </summary>
        /// <returns></returns>
        public SlashCommandCreationProperties Build()
        {
            if (BuiltCommand == null)
            {
                BuiltCommand = Builder.Build();
            }

            return BuiltCommand;
        }

        /// <summary>
        /// Check if given user has permission to execute this command
        /// </summary>
        /// <param name="user">User to check</param>
        /// <param name="resolver">Permission resolver to use for resolving correct permissions</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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
        
        /// <summary>
        /// Check if the command can be used by everyone
        /// </summary>
        /// <param name="resolver"></param>
        /// <param name="token"></param>
        /// <returns>Returns whether the command has assignment with DiscordTarget Everyone</returns>
        public Task<bool> IsForEveryoneAsync(IPermissionsResolver resolver, CancellationToken token = new CancellationToken())
        {
            return resolver.HasPermissionAsync(Permission, DiscordTarget.Everyone, token);
        }

        /// <summary>
        /// Refreshes command DefaultPermission and permissions
        /// </summary>
        /// <param name="resolver"></param>
        /// <param name="token"></param>
        /// <exception cref="InvalidOperationException"></exception>
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
            
            await ModifyCommand(resolver, token);
            await RefreshPermissions(resolver, token);
        }

        /// <summary>
        /// Registers commands to Discord
        /// </summary>
        /// <remarks>
        /// If the commands are found on discord, they will be modified instead of registering them again.
        /// For setting correct permissions <see cref="RegisterPermissionsAsync"/>
        /// </remarks>
        /// <param name="client"></param>
        /// <param name="resolver"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<IApplicationCommand> RegisterCommandAsync(DiscordRestClient client, IPermissionsResolver resolver, CancellationToken token = new CancellationToken())
        {
            if (Command == null)
            {
                SlashCommandCreationProperties command = await SetDefaultPermissionAsync(Build(), resolver, token);
                
                if (Global)
                {
                    Command = await CreateGlobalCommand(client, command, token);
                }
                else
                {
                    Command = await CreateGuildCommand(client, command, token);
                }
            }

            Registered = true;
            return Command;
        }

        /// <summary>
        /// Unregisters the discord command
        /// </summary>
        /// <param name="permissions"></param>
        /// <param name="token"></param>
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

        /// <summary>
        /// Registers correct permissions of the command
        /// </summary>
        /// <param name="client"></param>
        /// <param name="permissions"></param>
        /// <param name="token"></param>
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
        
                private async Task<RestApplicationCommand> CreateGlobalCommand(DiscordRestClient client, SlashCommandCreationProperties command, CancellationToken token = new CancellationToken())
        {
            RestApplication application = await client.GetApplicationInfoAsync();
            RestGlobalCommand? globalCommand = (await client.GetGlobalApplicationCommands(new RequestOptions() {CancelToken = token}))
                .FirstOrDefault(x => x.Name == command.Name && x.ApplicationId == application.Id);

            if (globalCommand != null)
            {
                await globalCommand.ModifyAsync(props =>
                {
                    props.Description = command.Description;
                    props.Options = command.Options;
                    props.DefaultPermission = command.DefaultPermission;
                });
            }
            
            globalCommand ??= await client.CreateGlobalCommand(command, new RequestOptions()
            {
                CancelToken = token
            });

            return globalCommand;
        }
        
        private async Task<RestApplicationCommand> CreateGuildCommand(DiscordRestClient client, SlashCommandCreationProperties command, CancellationToken token = new CancellationToken())
        {
            if (GuildId == null)
            {
                throw new ArgumentException("GuildId cannot be null for guild commands");
            }

            RestApplication application = await client.GetApplicationInfoAsync();
            RestGuildCommand? guildCommand = (await client.GetGuildApplicationCommands(GuildId.Value, new RequestOptions() {CancelToken = token}))
                .FirstOrDefault(x => x.Name == command.Name && x.ApplicationId == application.Id);

            if (guildCommand != null)
            {
                await guildCommand.ModifyAsync(props =>
                {
                    props.Description = command.Description;
                    props.Options = command.Options;
                    props.DefaultPermission = command.DefaultPermission;
                });
            }
            
            guildCommand ??= await client.CreateGuildCommand(command, GuildId.Value, new RequestOptions()
            {
                CancelToken = token
            });

            return guildCommand;
        }
    }
}