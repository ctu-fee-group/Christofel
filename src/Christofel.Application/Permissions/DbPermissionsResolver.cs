using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Enums;
using Christofel.BaseLib.Permissions;
using Microsoft.EntityFrameworkCore;

namespace Christofel.Application.Permissions
{
    /// <summary>
    /// Resolver of permissions using database.
    /// Table with PermissionName, DiscordTarget is used
    /// </summary>
    public sealed class DbPermissionsResolver : IPermissionsResolver
    {
        private ReadonlyDbContextFactory<ChristofelBaseContext> _readOnlyDbContextFactory;

        public DbPermissionsResolver(ReadonlyDbContextFactory<ChristofelBaseContext> readOnlyDbContextFactory)
        {
            _readOnlyDbContextFactory = readOnlyDbContextFactory;
        }

        public async Task<IEnumerable<DiscordTarget>> GetPermissionTargetsAsync(string permissionName, CancellationToken token = new CancellationToken())
        {
            await using var readOnlyContext = _readOnlyDbContextFactory.CreateDbContext();
            return await readOnlyContext.Set<PermissionAssignment>()
                .Where(x => GetPossiblePermissions(permissionName).Contains(x.PermissionName))
                .Select(x => x.Target)
                .ToListAsync(token);
        }

        public Task<IEnumerable<DiscordTarget>> GetPermissionTargetsAsync(IPermission permission, CancellationToken token = new CancellationToken())
        {
            return GetPermissionTargetsAsync(permission.Name, token);
        }

        public async Task<bool> HasPermissionAsync(string permissionName, DiscordTarget target, CancellationToken token = new CancellationToken())
        {
            await using var readOnlyContext = _readOnlyDbContextFactory.CreateDbContext();
            return await readOnlyContext.Set<PermissionAssignment>()
                .Where(x => GetPossiblePermissions(permissionName).Contains(x.PermissionName))
                .AnyAsync(x =>  x.Target.TargetType == TargetType.Everyone || 
                                (x.Target.DiscordId == target.DiscordId && x.Target.TargetType == target.TargetType), token);
        }

        public Task<bool> HasPermissionAsync(IPermission permission, DiscordTarget target, CancellationToken token = new CancellationToken())
        {
            return HasPermissionAsync(permission.Name, target, token);
        }

        public async Task<bool> AnyHasPermissionAsync(string permissionName, IEnumerable<DiscordTarget> target, CancellationToken token = new CancellationToken())
        {
            await using var readOnlyContext = _readOnlyDbContextFactory.CreateDbContext();
            return await readOnlyContext.Set<PermissionAssignment>()
                .Where(x => GetPossiblePermissions(permissionName).Contains(x.PermissionName))
                .AnyAsync(x =>  x.Target.TargetType == TargetType.Everyone || 
                                (target.Any(y => x.Target.DiscordId == y.DiscordId && x.Target.TargetType == y.TargetType)), token);
        }

        public Task<bool> AnyHasPermissionAsync(IPermission permission, IEnumerable<DiscordTarget> target, CancellationToken token = new CancellationToken())
        {
            return AnyHasPermissionAsync(permission.Name, target, token);
        }

        private IEnumerable<string> GetPossiblePermissions(string permissionName)
        {
            string[] splitted = permissionName.Split('.');

            yield return "*";

            string ret = "";
            foreach (string part in splitted.Take(splitted.Length - 1))
            {
                ret += part + ".";
                yield return ret + "*";
            }
            
            yield return permissionName;
        }
    }
}