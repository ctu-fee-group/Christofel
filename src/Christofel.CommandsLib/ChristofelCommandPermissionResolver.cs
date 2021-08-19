using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Permissions;
using Christofel.BaseLib.Extensions;
using Christofel.CommandsLib.Extensions;
using Discord;
using Discord.Net.Interactions.Abstractions;

namespace Christofel.CommandsLib
{
    public sealed class ChristofelCommandPermissionResolver : ICommandPermissionsResolver<PermissionSlashInfo>
    {
        private readonly IPermissionsResolver _permissionsResolver;
        
        public ChristofelCommandPermissionResolver(IPermissionsResolver permissionsResolver)
        {
            _permissionsResolver = permissionsResolver;
        }
        
        public Task<bool> IsForEveryoneAsync(PermissionSlashInfo info, CancellationToken cancellationToken)
        {
            return _permissionsResolver.HasPermissionAsync(info.Permission, DiscordTarget.Everyone, cancellationToken);
        }

        public Task<bool> HasPermissionAsync(IUser user, PermissionSlashInfo info, CancellationToken cancellationToken)
        {
            return _permissionsResolver.AnyHasPermissionAsync(info.Permission, user.GetAllDiscordTargets(),
                cancellationToken);
        }

        public Task<IEnumerable<ApplicationCommandPermission>> GetCommandPermissionsAsync(PermissionSlashInfo info, CancellationToken cancellationToken)
        {
            return _permissionsResolver.GetSlashCommandPermissionsAsync(info.Permission, cancellationToken);
        }
    }
}