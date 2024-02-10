//
//   DuplicateResolver.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.BaseLib.Extensions;
using Christofel.Common.Database;
using Christofel.Common.Database.Models;
using Christofel.Common.User;
using Christofel.CtuAuth.Auth;
using Christofel.Helpers.ReadOnlyDatabase;
using Microsoft.EntityFrameworkCore;

namespace Christofel.CtuAuth.Resolvers
{
    /// <summary>
    /// Resolver of duplicates of the specified user.
    /// </summary>
    public class DuplicateResolver
    {
        private readonly ReadonlyDbContextFactory<ChristofelBaseContext> _dbContextFactory;
        private readonly Dictionary<ILinkUser, CompositedDuplicates> _duplicates;

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateResolver"/> class.
        /// </summary>
        /// <param name="dbContextFactory">The read only christofel base database context factory.</param>
        public DuplicateResolver(ReadonlyDbContextFactory<ChristofelBaseContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            _duplicates = new Dictionary<ILinkUser, CompositedDuplicates>();
        }

        /// <summary>
        /// Finds duplicate by specified rules.
        /// </summary>
        /// <param name="user">What are the loaded information (from apis) about the user.</param>
        /// <param name="ct">The canellation token for the operation.</param>
        /// <returns>Duplicate information of the specified user.</returns>
        public async Task<CompositedDuplicates> ResolveDuplicateAsync(ILinkUser user, CancellationToken ct = default)
        {
            if (_duplicates.TryGetValue(user, out var async))
            {
                return async;
            }

            await using var dbContext = _dbContextFactory.CreateDbContext();

            var duplicateBoth = await dbContext.Set<DbUser>()
                .AsNoTracking()
                .AsQueryable()
                .Authenticated()
                .Where(x => x.CtuUsername == user.CtuUsername && x.DiscordId == user.DiscordId)
                .Select(x => new DuplicateUser(x.UserId, x.CtuUsername!, x.DiscordId))
                .ToListAsync(ct);

            var duplicateCtu = await dbContext.Set<DbUser>()
                .AsNoTracking()
                .AsQueryable()
                .Authenticated()
                .Where(x => x.CtuUsername == user.CtuUsername && x.DiscordId != user.DiscordId)
                .Select(x => new DuplicateUser(x.UserId, x.CtuUsername!, x.DiscordId))
                .ToListAsync(ct);

            var duplicateDiscord = await dbContext.Set<DbUser>()
                .AsNoTracking()
                .AsQueryable()
                .Authenticated()
                .Where(x => x.DiscordId == user.DiscordId && x.CtuUsername != user.CtuUsername)
                .Select(x => new DuplicateUser(x.UserId, x.CtuUsername!, x.DiscordId))
                .ToListAsync(ct);

            var both = duplicateBoth.Count > 0 ? new Duplicate(DuplicityType.Both, duplicateBoth) : null;
            var ctu = duplicateCtu.Count > 0 ? new Duplicate(DuplicityType.CtuSide, duplicateCtu) : null;
            var discord = duplicateDiscord.Count > 0 ? new Duplicate(DuplicityType.DiscordSide, duplicateDiscord) : null;

            return _duplicates[user] = new CompositedDuplicates
                (both is not null || ctu is not null || discord is not null, both, ctu, discord);
        }
    }
}