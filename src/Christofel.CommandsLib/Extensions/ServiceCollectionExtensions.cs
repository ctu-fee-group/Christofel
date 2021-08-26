using System;
using Christofel.CommandsLib.Validator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Remora.Commands.Extensions;
using Remora.Commands.Services;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Core;

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
                .AddScoped<ValidationFeedbackService>()
                .AddCondition<RequirePermissionCondition>()
                .AddPostExecutionEvent<ValidationErrorHandler>()
                .AddPostExecutionEvent<ErrorExecutionEvent>();
        }
    }
}