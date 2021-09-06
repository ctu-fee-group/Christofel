using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Enums;
using Christofel.BaseLib.Extensions;
using Christofel.BaseLib.Implementations.ReadOnlyDatabase;
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
        private readonly ReadonlyDbContextFactory<ChristofelBaseContext> _readOnlyDbContextFactory;

        public DbPermissionsResolver(ReadonlyDbContextFactory<ChristofelBaseContext> readOnlyDbContextFactory)
        {
            _readOnlyDbContextFactory = readOnlyDbContextFactory;
        }

        public async Task<IEnumerable<DiscordTarget>> GetPermissionTargetsAsync(string permissionName, CancellationToken token = new CancellationToken())
        {
            await using ReadOnlyDbContext readOnlyContext = _readOnlyDbContextFactory.CreateDbContext();
            return await readOnlyContext.Set<PermissionAssignment>()
                .Where(x => GetPossiblePermissions(permissionName).Contains(x.PermissionName))
                .Select(x => x.Target)
                .ToListAsync(token);
        }

        public async Task<bool> HasPermissionAsync(string permissionName, DiscordTarget target, CancellationToken token = new CancellationToken())
        {
            await using ReadOnlyDbContext readOnlyContext = _readOnlyDbContextFactory.CreateDbContext();
            return await readOnlyContext.Set<PermissionAssignment>()
                .Where(x => GetPossiblePermissions(permissionName).Contains(x.PermissionName))
                .AnyAsync(x =>  x.Target.TargetType == TargetType.Everyone || 
                                (x.Target.DiscordId == target.DiscordId && x.Target.TargetType == target.TargetType), token);
        }

        public async Task<bool> AnyHasPermissionAsync(string permissionName, IEnumerable<DiscordTarget> targets, CancellationToken token = new CancellationToken())
        {
            

            await using ReadOnlyDbContext readOnlyContext = _readOnlyDbContextFactory.CreateDbContext();
            return await readOnlyContext.Set<PermissionAssignment>()
                .Where(x => GetPossiblePermissions(permissionName).Contains(x.PermissionName))
                .WhereTargetAnyOf(targets)
                .AnyAsync(token);
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