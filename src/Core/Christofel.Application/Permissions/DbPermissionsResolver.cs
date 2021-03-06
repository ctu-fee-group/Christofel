//
//   DbPermissionsResolver.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Extensions;
using Christofel.Common.Database;
using Christofel.Common.Database.Models;
using Christofel.Common.Database.Models.Enums;
using Christofel.Common.Permissions;
using Christofel.Helpers.ReadOnlyDatabase;
using Microsoft.EntityFrameworkCore;

namespace Christofel.Application.Permissions
{
    /// <summary>
    /// Permission resolver that uses database for the resolution of the permissions.
    /// </summary>
    public sealed class DbPermissionsResolver : IPermissionsResolver
    {
        private readonly ReadonlyDbContextFactory<ChristofelBaseContext> _readOnlyDbContextFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbPermissionsResolver"/> class.
        /// </summary>
        /// <param name="readOnlyDbContextFactory">The readonly christofel base database context factory.</param>
        public DbPermissionsResolver(ReadonlyDbContextFactory<ChristofelBaseContext> readOnlyDbContextFactory)
        {
            _readOnlyDbContextFactory = readOnlyDbContextFactory;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DiscordTarget>> GetPermissionTargetsAsync
            (string permissionName, CancellationToken token = default)
        {
            await using IReadableDbContext readOnlyContext = _readOnlyDbContextFactory.CreateDbContext();
            return await readOnlyContext.Set<PermissionAssignment>()
                .Where(x => GetPossiblePermissions(permissionName).Contains(x.PermissionName))
                .Select(x => x.Target)
                .ToListAsync(token);
        }

        /// <inheritdoc />
        public async Task<bool> HasPermissionAsync
            (string permissionName, DiscordTarget target, CancellationToken token = default)
        {
            await using IReadableDbContext readOnlyContext = _readOnlyDbContextFactory.CreateDbContext();
            return await readOnlyContext.Set<PermissionAssignment>()
                .Where(x => GetPossiblePermissions(permissionName).Contains(x.PermissionName))
                .AnyAsync
                (
                    x => x.Target.TargetType == TargetType.Everyone ||
                         (x.Target.DiscordId == target.DiscordId && x.Target.TargetType == target.TargetType),
                    token
                );
        }

        /// <inheritdoc />
        public async Task<bool> AnyHasPermissionAsync
        (
            string permissionName,
            IEnumerable<DiscordTarget> targets,
            CancellationToken token = default
        )
        {
            await using IReadableDbContext readOnlyContext = _readOnlyDbContextFactory.CreateDbContext();
            return await readOnlyContext.Set<PermissionAssignment>()
                .Where(x => GetPossiblePermissions(permissionName).Contains(x.PermissionName))
                .WhereTargetAnyOf(targets)
                .AnyAsync(token);
        }

        private IEnumerable<string> GetPossiblePermissions(string permissionName)
        {
            string[] splitted = permissionName.Split('.');

            yield return "*";

            string ret = string.Empty;
            foreach (string part in splitted.Take(splitted.Length - 1))
            {
                ret += part + ".";
                yield return ret + "*";
            }

            yield return permissionName;
        }
    }
}