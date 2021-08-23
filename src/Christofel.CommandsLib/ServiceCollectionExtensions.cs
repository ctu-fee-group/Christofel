using Discord.Net.Interactions;
using Discord.Net.Interactions.Abstractions;
using Discord.Net.Interactions.DI;
using Discord.Net.Interactions.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Christofel.CommandsLib
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddChristofelInteractionService(this IServiceCollection collection, IConfiguration guildConfiguration)
        {
            return collection
                .AddSingleton<Discord.Net.Interactions.InteractionsService, InteractionsService>(
                    p => p.GetRequiredService<InteractionsService>())
                .AddSingleton<InteractionsService>()
                .AddSingleton<ICommandPermissionsResolver<PermissionSlashInfo>,
                    ChristofelCommandPermissionResolver>(p =>
                    p.GetRequiredService<ChristofelCommandPermissionResolver>())
                .AddOneGuildResolver<PermissionSlashInfo>(guildConfiguration)
                .AddSingleton<ChristofelCommandPermissionResolver>()
                .AddOneByOneCommandRegistrator<PermissionSlashInfo>()
                .AddDefaultInteractionService();
        }
    }
}