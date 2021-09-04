using System;
using Christofel.CommandsLib.ContextedParsers;
using Christofel.CommandsLib.Validator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Remora.Commands.Extensions;
using Remora.Commands.Services;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Core;

namespace Christofel.CommandsLib.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddChristofelCommands(this IServiceCollection collection)
        {
            return collection
                .AddDiscordCommands(true)
                .AddSingleton<ChristofelSlashService>()
                .AddScoped<ValidationFeedbackService>()
                .AddTransient<ChristofelCommandPermissionResolver>()
                .AddTransient<ChristofelCommandRegistrator>()
                // parsers
                .AddParser<ContextualUserParser>()
                .AddParser<ContextualGuildMemberParser>()
                .AddParser<ContextualRoleParser>()
                .AddParser<ContextualChannelParser>()
                // conditions
                .AddCondition<RequirePermissionCondition>()
                // execution events
                .AddPostExecutionEvent<ValidationErrorHandler>()
                .AddPostExecutionEvent<ErrorExecutionEvent>();
        }
    }
}