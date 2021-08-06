using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Extensions;
using Christofel.BaseLib.User;
using Microsoft.EntityFrameworkCore;

namespace Christofel.Management.CtuUtils
{
    public class CtuIdentityResolver
    {
        private record LinkUser(int UserId, string CtuUsername, ulong DiscordId) : ILinkUser;

        private readonly ReadonlyDbContextFactory<ChristofelBaseContext> _dbContextFactory;
        
        public CtuIdentityResolver(ReadonlyDbContextFactory<ChristofelBaseContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }
        
        public IAsyncEnumerable<DbUser> GetDuplicities(IReadableDbContext context, ulong userDiscordId)
        {
            return context.Set<DbUser>()
                .AsAsyncEnumerable()
                .Where(x => x.DiscordId == userDiscordId ||
                            (x.DuplicitUser != null && x.DuplicitUser.DiscordId == userDiscordId));
        }

        public IAsyncEnumerable<ulong> GetDuplicitiesDiscordIds(IReadableDbContext context, ulong userDiscordId)
        {
            return GetDuplicities(context, userDiscordId)
                .Select(x => x.DiscordId == userDiscordId ? (x.DuplicitUser?.DiscordId ?? 0) : x.DiscordId);
        }

        public async Task<List<ulong>> GetDuplicitiesDiscordIdsList(ulong userDiscordId, CancellationToken token = default)
        {
            await using IReadableDbContext context = _dbContextFactory.CreateDbContext();
            return await GetDuplicitiesDiscordIds(context, userDiscordId).ToListAsync(token);
        }

        public async Task<ILinkUser?> GetFirstIdentity(ulong userDiscordId)
        {
            await using IReadableDbContext context = _dbContextFactory.CreateDbContext();
            return await GetIdentities(context, userDiscordId).FirstOrDefaultAsync();
        }
        
        public IAsyncEnumerable<ILinkUser> GetIdentities(IReadableDbContext context, ulong userDiscordId)
        {
            return context.Set<DbUser>()
                .AsAsyncEnumerable()
                .Authenticated()
                .Select(x => new LinkUser(x.UserId, x.CtuUsername, x.DiscordId))
                .Where(x => userDiscordId == x.DiscordId);
        }

        public IAsyncEnumerable<ulong> GetIdentitiesDiscordIds(IReadableDbContext context, ulong userDiscordId)
        {
            return GetIdentities(context, userDiscordId)
                .Select(x => x.DiscordId);
        }
        
        public async Task<List<ulong>> GetIdentitiesDiscordIdsList(ulong userDiscordId)
        {
            await using IReadableDbContext context = _dbContextFactory.CreateDbContext();
            return await GetIdentitiesDiscordIds(context, userDiscordId)
                .ToListAsync();
        }
    }
}