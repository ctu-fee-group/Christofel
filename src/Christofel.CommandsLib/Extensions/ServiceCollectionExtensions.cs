using System;
using Christofel.CommandsLib.Validator;
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
                .AddTransient<ChristofelCommandRegistrator>()
                .AddDiscordCommands(true)
                .AddSingleton<ValidationFeedbackService>()
                .AddCondition<RequirePermissionCondition>()
                .AddPostExecutionEvent<ValidationErrorHandler>();
        }
    }
}