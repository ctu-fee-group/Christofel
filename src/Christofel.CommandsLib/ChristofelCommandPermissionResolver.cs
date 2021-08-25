using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Enums;
using Christofel.BaseLib.Extensions;
using Christofel.BaseLib.Permissions;
using Christofel.CommandsLib.Extensions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Remora.Commands.Trees.Nodes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Core;

namespace Christofel.CommandsLib
{
    public sealed class ChristofelCommandPermissionResolver
    {
        private readonly IPermissionsResolver _permissionsResolver;

        public ChristofelCommandPermissionResolver(IPermissionsResolver permissionsResolver)
        {
            _permissionsResolver = permissionsResolver;
        }

        public Task<bool> IsForEveryoneAsync(Snowflake? guildId, CommandNode commandNode,
            CancellationToken cancellationToken) =>
            IsForEveryoneAsync(guildId, commandNode.GetChristofelPermission(), cancellationToken);

        public Task<IEnumerable<IApplicationCommandPermissions>> GetCommandPermissionsAsync(Snowflake? guildId,
            CommandNode commandNode,
            CancellationToken cancellationToken) =>
            GetCommandPermissionsAsync(guildId, commandNode.GetChristofelPermission(), cancellationToken);

        public Task<bool> IsForEveryoneAsync(Snowflake? guildId, GroupNode commandNode,
            CancellationToken cancellationToken) =>
            IsForEveryoneAsync(guildId, commandNode.GetChristofelPermission(), cancellationToken);

        public Task<IEnumerable<IApplicationCommandPermissions>> GetCommandPermissionsAsync(Snowflake? guildId,
            GroupNode commandNode,
            CancellationToken cancellationToken) =>
            GetCommandPermissionsAsync(guildId, commandNode.GetChristofelPermission(), cancellationToken);

        public Task<bool> HasPermissionAsync(IGuildMember user, string permission,
            CancellationToken cancellationToken)
        {
            return _permissionsResolver.AnyHasPermissionAsync(permission, user.GetAllDiscordTargets(),
                cancellationToken);
        }

        public Task<bool> HasPermissionAsync(IUser user, string permission, CancellationToken cancellationToken)
        {
            return _permissionsResolver.AnyHasPermissionAsync(permission, new[] { user.ToDiscordTarget() },
                cancellationToken);
        }
        
        public Task<bool> HasPermissionAsync(Snowflake userId, IReadOnlyList<Snowflake> roleIds, string permission, CancellationToken cancellationToken)
        {
            List<DiscordTarget> targets = new List<DiscordTarget>();
            targets.Add(new DiscordTarget(userId.Value, TargetType.User));
            targets.Add(DiscordTarget.Everyone);
            targets.AddRange(roleIds.Select(x => new DiscordTarget(x.Value, TargetType.Role)));

            return _permissionsResolver.AnyHasPermissionAsync(permission, targets,
                cancellationToken);
        }

        public Task<bool> IsForEveryoneAsync(Snowflake? guildId, string? permission, CancellationToken cancellationToken)
        {
            if (permission is null)
            {
                return Task.FromResult(true);
            }

            return _permissionsResolver.HasPermissionAsync(
                permission, DiscordTarget.Everyone,
                cancellationToken);
        }

        public Task<IEnumerable<IApplicationCommandPermissions>> GetCommandPermissionsAsync(Snowflake? guildId, string? permission,
            CancellationToken cancellationToken)
        {
            if (permission is null)
            {
                return Task.FromResult(Enumerable.Empty<IApplicationCommandPermissions>());
            }

            return _permissionsResolver.GetSlashCommandPermissionsAsync(permission, cancellationToken);
        }
    }
}