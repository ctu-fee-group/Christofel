using System;
using System.Threading.Tasks;
using Christofel.BaseLib.Database;
using Christofel.DatabaseMigrator.Model;
using Christofel.DatabaseMigrator.ModelMigrator;
using Christofel.ReactHandler.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Christofel.DatabaseMigrator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("config.json")
                .Build();

            var services = new ServiceCollection()
                .AddSingleton<Migrator>()
                .AddLogging(b => b.ClearProviders().AddConsole())
                .AddTransient<IModelMigrator, UserMigrator>()
                .AddTransient<IModelMigrator, UserMigrator>()
                .AddTransient<IModelMigrator, UserMigrator>()
                .AddTransient<ObtainMessageChannel>()
                .AddDbContext<OldContext>(o => o.UseSqlite(configuration.GetConnectionString("Old")))
                .AddDbContext<ChristofelBaseContext>(options =>
                    options
                        .UseMySql(
                            configuration.GetConnectionString("ChristofelBase"),
                            ServerVersion.AutoDetect(configuration.GetConnectionString("ChristofelBase")
                            ))
                )
                .AddDbContext<ReactHandlerContext>(options =>
                    options
                        .UseMySql(
                            configuration.GetConnectionString("ReactHandler"),
                            ServerVersion.AutoDetect(configuration.GetConnectionString("ReactHandler")
                            ))
                )
                .BuildServiceProvider();

            await services.GetRequiredService<Migrator>().Migrate();
        }
    }
}