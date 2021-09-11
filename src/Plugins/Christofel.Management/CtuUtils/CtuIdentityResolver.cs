//
//   CtuIdentityResolver.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Extensions;
using Christofel.BaseLib.Implementations.ReadOnlyDatabase;
using Christofel.BaseLib.User;
using Microsoft.EntityFrameworkCore;
using Remora.Discord.Core;

namespace Christofel.Management.CtuUtils
{
    /// <summary>
    /// Service to resolve CTU identity or duplicates for a given user.
    /// </summary>
    public class CtuIdentityResolver
    {
        private readonly ReadonlyDbContextFactory<ChristofelBaseContext> _dbContextFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="CtuIdentityResolver"/> class.
        /// </summary>
        /// <param name="dbContextFactory">The Christofel base database context factory.</param>
        public CtuIdentityResolver(ReadonlyDbContextFactory<ChristofelBaseContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        /// <summary>
        /// Finds all duplicates of the given user.
        /// </summary>
        /// <param name="context">The context where to search in.</param>
        /// <param name="userDiscordId">The id of the user to find duplicates for.</param>
        /// <returns>A queryable that filters out all the duplicates of the specified user.</returns>
        public IQueryable<DbUser> GetDuplicities(IReadableDbContext context, Snowflake userDiscordId)
        {
            return context.Set<DbUser>()
                .AsQueryable()
                .Where
                (
                    x => x.DiscordId == userDiscordId ||
                         (x.DuplicitUser != null &&
                          x.DuplicitUser.DiscordId == userDiscordId &&
                          x.AuthenticatedAt != null)
                );
        }

        /// <summary>
        /// Finds discord ids of all of the duplicates of the given user.
        /// </summary>
        /// <param name="context">The context where to search in.</param>
        /// <param name="userDiscordId">The id of the user to find duplicates for.</param>
        /// <returns>A queryable that filters out all the duplicates of the specified user.</returns>
        public IQueryable<Snowflake> GetDuplicitiesDiscordIds(IReadableDbContext context, Snowflake userDiscordId)
        {
            return GetDuplicities(context, userDiscordId)
                .Where(x => x.DuplicitUser != null || x.DiscordId != userDiscordId)
                .Select
                (
                    x => x.DiscordId == userDiscordId
                        ? x.DuplicitUser!.DiscordId
                        : x.DiscordId
                );
        }

        /// <summary>
        /// Finds discord ids of all of the duplicates of the given user.
        /// </summary>
        /// <param name="userDiscordId">The id of the user to find duplicates for.</param>
        /// <param name="token">The cancellation token for the operation.</param>
        /// <returns>A list containing all duplicities discord ids.</returns>
        public async Task<List<Snowflake>> GetDuplicitiesDiscordIdsList
            (Snowflake userDiscordId, CancellationToken token = default)
        {
            await using IReadableDbContext context = _dbContextFactory.CreateDbContext();
            return await GetDuplicitiesDiscordIds(context, userDiscordId).ToListAsync(token);
        }

        /// <summary>
        /// Finds the first matching identity of the given user.
        /// </summary>
        /// <param name="userDiscordId">The id of the user to find identity of.</param>
        /// <returns>A user that represents the identity of the given user, if any.</returns>
        public async Task<ILinkUser?> GetFirstIdentity(Snowflake userDiscordId)
        {
            await using IReadableDbContext context = _dbContextFactory.CreateDbContext();
            return await GetIdentities(context, userDiscordId).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Finds all of the identities of the given Discord user.
        /// </summary>
        /// <param name="context">The database context holding users.</param>
        /// <param name="userDiscordId">The id of the user to find duplicates for.</param>
        /// <returns>A queryable that filters out identities of the specified user discord id.</returns>
        public IQueryable<ILinkUser> GetIdentities(IReadableDbContext context, Snowflake userDiscordId)
        {
            return context.Set<DbUser>()
                .AsQueryable()
                .Authenticated()
                .Where(x => x.CtuUsername != null)
                .Select
                (
                    x => new LinkUser
                    {
#pragma warning disable 8600
                        CtuUsername = x.CtuUsername!,
#pragma warning restore 8600
                        UserId = x.UserId,
                        DiscordId = x.DiscordId
                    }
                )
                .Where(x => userDiscordId == x.DiscordId);
        }

        /// <summary>
        /// Finds the ctu usernames of the identities of the given user.
        /// </summary>
        /// <param name="context">The database context holding users.</param>
        /// <param name="userDiscordId">The id of the user to find identities of.</param>
        /// <returns>A queryable with filtered identities of the user.</returns>
        public IQueryable<string> GetIdentitiesCtuUsernames(IReadableDbContext context, Snowflake userDiscordId)
        {
            return GetIdentities(context, userDiscordId)
                .Select(x => x.CtuUsername);
        }

        /// <summary>
        /// Finds the ctu usernames of the identities of the given user.
        /// </summary>
        /// <param name="userDiscordId">The id of the user to find identities of.</param>
        /// <returns>A list with filtered identities of the user.</returns>
        public async Task<List<string>> GetIdentitiesCtuUsernamesList(Snowflake userDiscordId)
        {
            await using IReadableDbContext context = _dbContextFactory.CreateDbContext();
            return await GetIdentitiesCtuUsernames(context, userDiscordId)
                .ToListAsync();
        }

        private class LinkUser : ILinkUser
        {
            public int UserId { get; init; }

            public string CtuUsername { get; init; } = null!;

            public Snowflake DiscordId { get; init; }
        }
    }
}