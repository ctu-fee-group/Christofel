//
//   CtuAuthProcessConditionTests.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
    /// <summary>
    /// Tests of real conditions of the ctu auth process.
    /// </summary>
    /// <typeparam name="T">Type of the condition.</typeparam>
    public class CtuAuthProcessConditionTests<T> : IDisposable
        where T : class, IPreAuthCondition
    {
        /// <summary>
        /// Gets the database context.
        /// </summary>
        protected ChristofelBaseContext DbContext { get; }

        /// <summary>
        /// Gets dummy access token used for testing.
        /// </summary>
        protected string DummyAccessToken => "myToken";

        /// <summary>
        /// Gets dummy guild id used for testing.
        /// </summary>
        protected ulong DummyGuildId => 93249823482348;

        /// <summary>
        /// Gets dummy username used for testing.
        /// </summary>
        protected string DummyUsername => "someUsername";

        /// <summary>
        /// Gets options of the database context.
        /// </summary>
        protected DbContextOptionsDisposable<ChristofelBaseContext> OptionsDisposable { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CtuAuthProcessConditionTests{T}"/> class.
        /// </summary>
        public CtuAuthProcessConditionTests()
        {
            var options = SqliteInMemory.CreateOptions<ChristofelBaseContext>();
            OptionsDisposable = options;

            DbContext = new ChristofelBaseContext(options);
            DbContext.Database.EnsureCreated();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            DbContext?.Dispose();
            OptionsDisposable?.Dispose();
        }

        /// <summary>
        /// Setups service provider with the condition added.
        /// </summary>
        /// <param name="configure">Action to configure the collection.</param>
        /// <returns>Service provider with ctu auth services.</returns>
        protected virtual IServiceProvider SetupConditionServices(Action<IServiceCollection>? configure = default)
        {
            var services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddAuthCondition<T>()
                .AddTransient(p => p.GetRequiredService<IDbContextFactory<ChristofelBaseContext>>().CreateDbContext())
                .AddSingleton<IDbContextFactory<ChristofelBaseContext>, ChristofelBaseContextFactory>
                    (p => new ChristofelBaseContextFactory(OptionsDisposable))
                .AddSingleton<ReadonlyDbContextFactory<ChristofelBaseContext>>()
                .AddLogging(b => b.ClearProviders());

            configure?.Invoke(services);

            return services
                .BuildServiceProvider();
        }
    }
}