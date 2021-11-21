//
//   ChristofelCommandPermissionResolver.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Extensions;
using Christofel.CommandsLib.Extensions;
using Christofel.Common.Database.Models;
using Christofel.Common.Database.Models.Enums;
using Christofel.Common.Permissions;
using Remora.Commands.Trees.Nodes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Rest.Core;

namespace Christofel.CommandsLib.Permissions
{
    /// <summary>
    /// Permission resolver for Remora.Commands using <see cref="RequirePermissionAttribute"/>
    /// on the commands or groups.
    /// </summary>
    public sealed class ChristofelCommandPermissionResolver
    {
        private readonly IPermissionsResolver _permissionsResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChristofelCommandPermissionResolver"/> class.
        /// </summary>
        /// <param name="permissionsResolver">The resolver of the Christofel permissions.</param>
        public ChristofelCommandPermissionResolver(IPermissionsResolver permissionsResolver)
        {
            _permissionsResolver = permissionsResolver;
        }

        /// <summary>
        /// Whether the given command can be executed by anyone.
        /// </summary>
        /// <param name="guildId">The guild id.</param>
        /// <param name="commandNode">The command to check.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>Whether the command may be executed by everyone.</returns>
        public Task<bool> IsForEveryoneAsync
        (
            Snowflake? guildId,
            CommandNode commandNode,
            CancellationToken cancellationToken
        ) =>
            IsForEveryoneAsync(guildId, commandNode.GetChristofelPermission(), cancellationToken);

        /// <summary>
        /// Get all permissions that should be assigned to the given command.
        /// </summary>
        /// <param name="guildId">The guild id.</param>
        /// <param name="commandNode">The command node to check.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>List of permissions that should be assigned to the command.</returns>
        public Task<IEnumerable<IApplicationCommandPermissions>> GetCommandPermissionsAsync
        (
            Snowflake? guildId,
            CommandNode commandNode,
            CancellationToken cancellationToken
        ) =>
            GetCommandPermissionsAsync(guildId, commandNode.GetChristofelPermission(), cancellationToken);

        /// <summary>
        /// Whether the given group can be executed by anyone.
        /// </summary>
        /// <param name="guildId">The guild id.</param>
        /// <param name="commandNode">The group to check.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>Whether the group may be executed by everyone.</returns>
        public Task<bool> IsForEveryoneAsync
        (
            Snowflake? guildId,
            GroupNode commandNode,
            CancellationToken cancellationToken
        ) =>
            IsForEveryoneAsync(guildId, commandNode.GetChristofelPermission(), cancellationToken);

        /// <summary>
        /// Get all permissions that should be assigned to the given group.
        /// </summary>
        /// <param name="guildId">The guild id.</param>
        /// <param name="commandNode">The group node to check.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>List of permissions that should be assigned to the group.</returns>
        public Task<IEnumerable<IApplicationCommandPermissions>> GetCommandPermissionsAsync
        (
            Snowflake? guildId,
            GroupNode commandNode,
            CancellationToken cancellationToken
        ) =>
            GetCommandPermissionsAsync(guildId, commandNode.GetChristofelPermission(), cancellationToken);

        /// <summary>
        /// Get whether the given guild member should be able to execute command with the given command.
        /// </summary>
        /// <param name="user">The member to check permissions of.</param>
        /// <param name="permission">The permission to check.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>Whether the given member has the given permission.</returns>
        public Task<bool> HasPermissionAsync
        (
            IGuildMember user,
            string permission,
            CancellationToken cancellationToken
        )
            => _permissionsResolver.AnyHasPermissionAsync
            (
                permission,
                user.GetAllDiscordTargets(),
                cancellationToken
            );

        /// <summary>
        /// Get whether the given user should be able to execute command with the given permission.
        /// </summary>
        /// <param name="user">The user to check permissions of.</param>
        /// <param name="permission">The permission to check.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>Whether the given user has the given permission.</returns>
        public Task<bool> HasPermissionAsync(IUser user, string permission, CancellationToken cancellationToken)
        {
            return _permissionsResolver.AnyHasPermissionAsync
            (
                permission,
                new[] { user.ToDiscordTarget() },
                cancellationToken
            );
        }

        /// <summary>
        /// Get whether the given user or any of the roles should be able to execute command with the given permission.
        /// </summary>
        /// <param name="userId">The user to check permissions of.</param>
        /// <param name="roleIds">The roles to check permissions of.</param>
        /// <param name="permission">The permission to check.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>Whether the given user or any of the roles have the given permission.</returns>
        public Task<bool> HasPermissionAsync
            (Snowflake userId, IReadOnlyList<Snowflake> roleIds, string permission, CancellationToken cancellationToken)
        {
            List<DiscordTarget> targets = new List<DiscordTarget>();
            targets.Add(new DiscordTarget(userId.Value, TargetType.User));
            targets.Add(DiscordTarget.Everyone);
            targets.AddRange(roleIds.Select(x => new DiscordTarget(x.Value, TargetType.Role)));

            return _permissionsResolver.AnyHasPermissionAsync
            (
                permission,
                targets,
                cancellationToken
            );
        }

        /// <summary>
        /// Whether the command given by permission can be executed by anyone in the given guild.
        /// </summary>
        /// <param name="guildId">The guild id.</param>
        /// <param name="permission">The permission to check.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>Whether the given permission is given to everyone.</returns>
        public Task<bool> IsForEveryoneAsync
            (Snowflake? guildId, string? permission, CancellationToken cancellationToken)
        {
            if (permission is null)
            {
                return Task.FromResult(true);
            }

            return _permissionsResolver.HasPermissionAsync
            (
                permission,
                DiscordTarget.Everyone,
                cancellationToken
            );
        }

        /// <summary>
        /// Get all permissions that should be assigned based on the given permission.
        /// </summary>
        /// <param name="guildId">The guild id.</param>
        /// <param name="permission">The permission to return information about.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>List of permissions that should be assigned to slash command based on the given permission.</returns>
        public Task<IEnumerable<IApplicationCommandPermissions>> GetCommandPermissionsAsync
        (
            Snowflake? guildId,
            string? permission,
            CancellationToken cancellationToken
        )
        {
            if (permission is null)
            {
                return Task.FromResult(Enumerable.Empty<IApplicationCommandPermissions>());
            }

            return _permissionsResolver.GetSlashCommandPermissionsAsync(permission, cancellationToken);
        }
    }
}