//
//   ServiceCollectionExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.CommandsLib.ContextedParsers;
using Christofel.CommandsLib.ExecutionEvents;
using Christofel.CommandsLib.Permissions;
using Christofel.CommandsLib.Validator;
using Microsoft.Extensions.DependencyInjection;
using Remora.Commands.Extensions;
using Remora.Discord.Commands.Extensions;

namespace Christofel.CommandsLib.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddChristofelCommands(this IServiceCollection collection) => collection
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
            .AddParser<SnowflakeParser>()
            // conditions
            .AddCondition<RequirePermissionCondition>()
            // execution events
            .AddPostExecutionEvent<WrongParametersExecutionEvent>()
            .AddPostExecutionEvent<ValidationErrorHandler>()
            .AddPostExecutionEvent<ErrorExecutionEvent>()
            .AddPostExecutionEvent<ParsingErrorExecutionEvent>();
    }
}