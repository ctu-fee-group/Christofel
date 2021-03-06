//
//   DuplicateResolver.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu.Auth;
using Christofel.BaseLib.Extensions;
using Christofel.Common.Database;
using Christofel.Common.Database.Models;
using Christofel.Common.User;
using Christofel.Helpers.ReadOnlyDatabase;
using Microsoft.EntityFrameworkCore;

namespace Christofel.Api.Ctu.Resolvers
{
    /// <summary>
    /// Resolver of duplicates of the specified user.
    /// </summary>
    public class DuplicateResolver
    {
        private readonly ReadonlyDbContextFactory<ChristofelBaseContext> _dbContextFactory;
        private readonly Dictionary<ILinkUser, Duplicate> _duplicates;

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateResolver"/> class.
        /// </summary>
        /// <param name="dbContextFactory">The read only christofel base database context factory.</param>
        public DuplicateResolver(ReadonlyDbContextFactory<ChristofelBaseContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            _duplicates = new Dictionary<ILinkUser, Duplicate>();
        }

        /// <summary>
        /// Finds duplicate by specified rules.
        /// </summary>
        /// <param name="user">What are the loaded information (from apis) about the user.</param>
        /// <param name="ct">The canellation token for the operation.</param>
        /// <returns>Duplicate information of the specified user.</returns>
        public async Task<Duplicate> ResolveDuplicateAsync(ILinkUser user, CancellationToken ct = default)
        {
            if (_duplicates.ContainsKey(user))
            {
                return _duplicates[user];
            }

            await using var dbContext = _dbContextFactory.CreateDbContext();
            Duplicate? foundDuplicate = null;

            var duplicateBoth = await dbContext.Set<DbUser>()
                .AsQueryable()
                .Authenticated()
                .Where(x => x.CtuUsername == user.CtuUsername && x.DiscordId == user.DiscordId)
                .FirstOrDefaultAsync(ct);

            if (duplicateBoth is not null)
            {
                foundDuplicate = new Duplicate(DuplicityType.Both, duplicateBoth);
            }

            if (foundDuplicate is null)
            {
                var duplicateDiscord = await dbContext.Set<DbUser>()
                    .AsQueryable()
                    .Authenticated()
                    .Where(x => x.DiscordId == user.DiscordId)
                    .FirstOrDefaultAsync(ct);

                if (duplicateDiscord is not null)
                {
                    foundDuplicate = new Duplicate(DuplicityType.DiscordSide, duplicateDiscord);
                }
            }

            if (foundDuplicate is null)
            {
                var duplicateCtu = await dbContext.Set<DbUser>()
                    .AsQueryable()
                    .Authenticated()
                    .Where(x => x.CtuUsername == user.CtuUsername)
                    .FirstOrDefaultAsync(ct);

                if (duplicateCtu is not null)
                {
                    foundDuplicate = new Duplicate(DuplicityType.CtuSide, duplicateCtu);
                }
            }

            return _duplicates[user] = foundDuplicate ?? new Duplicate(DuplicityType.None, null);
        }
    }
}