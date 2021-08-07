using System;
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

namespace Christofel.CommandsLib.CommandsInfo
{
    public delegate Task SlashCommandHandler(SocketSlashCommand command, CancellationToken token = new CancellationToken());

    /// <summary>
    /// Information about a slash command
    /// </summary>
    public class SlashCommandInfo
    {
        public SlashCommandInfo(SlashCommandBuilder builder, string permission,
            SlashCommandHandler handler)
        {
            BuiltCommand = builder.Build();
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
        /// Built command that is set after calling Build()
        /// </summary>
        public SlashCommandCreationProperties BuiltCommand { get; private set; }
        
        /// <summary>
        /// Registered command that is set after calling RegisterCommandAsync
        /// </summary>
        public RestApplicationCommand? Command { get; set; }
        
        /// <summary>
        /// Permission of the command
        /// </summary>
        public CommandPermission Permission { get; }

        /// <summary>
        /// Check if given user has permission to execute this command
        /// </summary>
        /// <param name="user">User to check</param>
        /// <param name="resolver">Permission resolver to use for resolving correct permissions</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<bool> HasPermissionAsync(SocketUser user, IPermissionsResolver resolver, CancellationToken cancellationToken = new CancellationToken())
        {
            return resolver.AnyHasPermissionAsync(Permission, user.GetAllDiscordTargets(), cancellationToken);
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
    }
}