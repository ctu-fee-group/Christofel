using Discord.Net.Interactions;
using Discord.Net.Interactions.Abstractions;
using Discord.Net.Interactions.DI;
using Discord.Net.Interactions.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Christofel.CommandsLib
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddChristofelInteractionService(this IServiceCollection collection)
        {
            return collection
                .AddSingleton<InteractionsService<PermissionSlashInfo>, InteractionsService>(
                    p => p.GetRequiredService<InteractionsService>())
                .AddSingleton<InteractionsService>()
                .AddSingleton<ICommandPermissionsResolver<PermissionSlashInfo>,
                    ChristofelCommandPermissionResolver>(p =>
                    p.GetRequiredService<ChristofelCommandPermissionResolver>())
                .AddSingleton<ChristofelCommandPermissionResolver>()
                .AddOneByOneCommandRegistrator<PermissionSlashInfo>()
                .AddDefaultInteractionService<PermissionSlashInfo>();
        }
    }
}