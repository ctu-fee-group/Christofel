using System;
using Christofel.Api.Ctu.Auth.Conditions;
using Christofel.Api.Ctu.Extensions;
using Christofel.Api.Tests.Data.Ctu.Auth;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Implementations.ReadOnlyDatabase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestSupport.EfHelpers;

namespace Christofel.Api.Tests.Ctu.Auth
{
    public class CtuAuthProcessConditionTests<T> : IDisposable
        where T : class, IPreAuthCondition
    {
        protected readonly ChristofelBaseContext _dbContext;
        protected readonly DbContextOptionsDisposable<ChristofelBaseContext> _optionsDisposable;

        protected readonly string _dummyAccessToken = "myToken";
        protected readonly string _dummyUsername = "someUsername";
        protected readonly ulong _dummyGuildId = 93249823482348;

        public CtuAuthProcessConditionTests()
        {
            var options = SqliteInMemory.CreateOptions<ChristofelBaseContext>();
            _optionsDisposable = options;

            _dbContext = new ChristofelBaseContext(options);
            _dbContext.Database.EnsureCreated();
        }

        protected virtual IServiceProvider SetupConditionServices(Action<IServiceCollection>? configure = default)
        {
            var services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddAuthCondition<T>()
                .AddTransient<ChristofelBaseContext>(p => p.GetRequiredService<IDbContextFactory<ChristofelBaseContext>>().CreateDbContext())
                .AddSingleton<IDbContextFactory<ChristofelBaseContext>, ChristofelBaseContextFactory>(p => new ChristofelBaseContextFactory(_optionsDisposable))
                .AddSingleton<ReadonlyDbContextFactory<ChristofelBaseContext>>()
                .AddLogging(b => b.ClearProviders());

            configure?.Invoke(services);

            return services
                .BuildServiceProvider();
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
            _optionsDisposable?.Dispose();
        }
    }
}