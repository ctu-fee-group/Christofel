using Christofel.BaseLib;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Discord;
using Christofel.BaseLib.Lifetime;
using Christofel.BaseLib.Permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Christofel.Application.State
{
    public class ChristofelState : IChristofelState
    {
        public ChristofelState(IBot bot, IDbContextFactory<ChristofelBaseContext> factory,
            ReadonlyDbContextFactory<ChristofelBaseContext> readOnlyDatabaseFactory, IConfiguration config,
            IPermissionService permissions, ILoggerFactory loggerFactory, IApplicationLifetime lifetime)
        {
            Bot = bot;
            DatabaseFactory = factory;
            Configuration = config;
            Permissions = permissions;
            ReadOnlyDatabaseFactory = readOnlyDatabaseFactory;
            LoggerFactory = loggerFactory;
            Lifetime = lifetime;
        }
        
        public IBot Bot { get; }
        public IDbContextFactory<ChristofelBaseContext> DatabaseFactory { get; }
        public ReadonlyDbContextFactory<ChristofelBaseContext> ReadOnlyDatabaseFactory { get; }
        public IConfiguration Configuration { get; }
        public IPermissionService Permissions { get; }
        
        public ILoggerFactory LoggerFactory { get; }
        
        public IApplicationLifetime Lifetime { get; }
    }
}