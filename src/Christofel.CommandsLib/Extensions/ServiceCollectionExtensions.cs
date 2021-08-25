using System;
using Microsoft.Extensions.DependencyInjection;
using Remora.Commands.Extensions;
using Remora.Discord.Commands.Extensions;

namespace Christofel.CommandsLib.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddChristofelCommands(this IServiceCollection collection)
        {
            return collection
                .AddSingleton<ChristofelSlashService>()
                .AddTransient<ChristofelCommandPermissionResolver>()
                .AddDiscordCommands(true)
                .AddTransient<ChristofelCommandRegistrator>()
                .AddCondition<RequirePermissionCondition>();
        }
    }
}