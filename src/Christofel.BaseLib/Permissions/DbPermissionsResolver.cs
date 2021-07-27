using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Christofel.BaseLib.Permissions
{
    public sealed class DbPermissionsResolver : IPermissionsResolver
    {
        private ReadonlyDbContextFactory<ChristofelBaseContext> _readOnlyDbContextFactory;

        public DbPermissionsResolver(ReadonlyDbContextFactory<ChristofelBaseContext> readOnlyDbContextFactory)
        {
            _readOnlyDbContextFactory = readOnlyDbContextFactory;
        }

        public async Task<IEnumerable<DiscordTarget>> GetPermissionTargetsAsync(string permissionName)
        {
            await using var readOnlyContext = _readOnlyDbContextFactory.CreateDbContext();
            return await readOnlyContext.Set<PermissionAssignment>()
                .Where(x => GetPossiblePermissions(permissionName).Contains(x.PermissionName))
                .Select(x => x.Target)
                .ToListAsync();
        }

        public Task<IEnumerable<DiscordTarget>> GetPermissionTargetsAsync(IPermission permission)
        {
            return GetPermissionTargetsAsync(permission.Name);
        }

        public async Task<bool> HasPermissionAsync(string permissionName, DiscordTarget target)
        {
            await using var readOnlyContext = _readOnlyDbContextFactory.CreateDbContext();
            return await readOnlyContext.Set<PermissionAssignment>()
                .Where(x => GetPossiblePermissions(permissionName).Contains(x.PermissionName))
                .AnyAsync(x =>  x.Target.TargetType == TargetType.Everyone || 
                                (x.Target.DiscordId == target.DiscordId && x.Target.TargetType == target.TargetType));
        }

        public Task<bool> HasPermissionAsync(IPermission permission, DiscordTarget target)
        {
            return HasPermissionAsync(permission.Name, target);
        }

        public async Task<bool> AnyHasPermissionAsync(string permissionName, IEnumerable<DiscordTarget> target)
        {
            await using var readOnlyContext = _readOnlyDbContextFactory.CreateDbContext();
            return await readOnlyContext.Set<PermissionAssignment>()
                .Where(x => GetPossiblePermissions(permissionName).Contains(x.PermissionName))
                .AnyAsync(x =>  x.Target.TargetType == TargetType.Everyone || 
                                (target.Any(y => x.Target.DiscordId == y.DiscordId && x.Target.TargetType == y.TargetType)));
        }

        public Task<bool> AnyHasPermissionAsync(IPermission permission, IEnumerable<DiscordTarget> target)
        {
            return AnyHasPermissionAsync(permission.Name, target);
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