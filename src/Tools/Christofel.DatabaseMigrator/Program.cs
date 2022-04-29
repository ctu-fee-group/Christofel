using System.Threading.Tasks;
using Christofel.BaseLib.Extensions;
using Christofel.Common.Database;
using Christofel.DatabaseMigrator.Model;
using Christofel.DatabaseMigrator.ModelMigrator;
using Christofel.ReactHandler.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remora.Discord.Rest.Extensions;

namespace Christofel.DatabaseMigrator
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("config.json")
                .Build();

            var services = new ServiceCollection()
                .AddSingleton<Migrator>()
                .AddLogging(b => b.ClearProviders().AddConsole())
                .AddTransient<IModelMigrator, UserMigrator>()
                .AddTransient<IModelMigrator, MessageChannelMigrator>()
                .AddTransient<IModelMigrator, MessageRoleMigrator>()
                .AddTransient<ObtainMessageChannel>()
                .Configure<MigrationChannelOptions>(configuration.GetSection("Migration"))
                .AddDiscordRest(_ => configuration.GetValue<string>("Bot:Token"))
                .AddDbContext<OldContext>(o => o.UseSqlite(configuration.GetConnectionString("Old")))
                .AddChristofelDbContextFactory<ReactHandlerContext>(configuration)
                .AddChristofelDbContextFactory<ChristofelBaseContext>(configuration)
                .BuildServiceProvider();

            await services.GetRequiredService<Migrator>().Migrate();
        }
    }
}