using Christofel.BaseLib;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Discord;
using Christofel.BaseLib.Permissions;
using Microsoft.EntityFrameworkCore;

namespace Christofel.Application
{
    public class ChristofelState : IChristofelState
    {
        public ChristofelState(IBot bot, IDbContextFactory<ChristofelBaseContext> factory,
            ReadonlyDbContextFactory<ChristofelBaseContext> readOnlyDatabaseFactory, IReadableConfig config,
            IPermissionService permissions)
        {
            Bot = bot;
            DatabaseFactory = factory;
            Configuration = config;
            Permissions = permissions;
            ReadOnlyDatabaseFactory = readOnlyDatabaseFactory;
        }
        
        public IBot Bot { get; }
        public IDbContextFactory<ChristofelBaseContext> DatabaseFactory { get; }
        public ReadonlyDbContextFactory<ChristofelBaseContext> ReadOnlyDatabaseFactory { get; }
        public IReadableConfig Configuration { get; }
        public IPermissionService Permissions { get; }
    }
}