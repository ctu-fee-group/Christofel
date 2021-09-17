//
//   ServiceCollectionExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.CommandsLib.ContextedParsers;
using Christofel.CommandsLib.ExecutionEvents;
using Christofel.CommandsLib.Permissions;
using Christofel.CommandsLib.Validator;
using Christofel.Plugins;
using Christofel.Plugins.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Remora.Commands.Extensions;
using Remora.Discord.Commands.Extensions;

namespace Christofel.CommandsLib.Extensions
{
    /// <summary>
    /// Class containing extensions for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services for slash commands with support for contextual parsers and permissions.
        /// </summary>
        /// <remarks>
        /// Adds support for Remora discord command and <see cref="ChristofelSlashService"/>
        /// for managing slash commands.
        ///
        /// Adds execution events <see cref="WrongParametersExecutionEvent"/>,
        /// <see cref="ValidationErrorHandler"/>, <see cref="ErrorExecutionEvent"/>, <see cref="ParsingErrorExecutionEvent"/>
        ///
        /// Adds <see cref="ChristofelCommandRegistrator"/> as a <see cref="IStartable"/>,
        /// <see cref="IRefreshable"/>, <see cref="IStoppable"/> service for registering
        /// the commands using <see cref="ChristofelSlashService"/>.
        ///
        /// Adds contextual parsers for less resource heavy parsing
        /// <see cref="ContextualChannelParser"/>, <see cref="ContextualRoleParser"/>,
        /// <see cref="ContextualUserParser"/>, <see cref="ContextualGuildMemberParser"/>.
        ///
        /// Adds <see cref="ValidationFeedbackService"/> for generating embeds with wrong validation data.
        /// </remarks>
        /// <param name="collection">The collection to add commands into.</param>
        /// <returns>The passed collection.</returns>
        public static IServiceCollection AddChristofelCommands(this IServiceCollection collection) => collection
            .AddDiscordCommands(true)
            .AddSingleton<ChristofelSlashService>()
            .AddScoped<ValidationFeedbackService>()
            .AddTransient<ChristofelCommandPermissionResolver>()
            .AddStateful<ChristofelCommandRegistrator>(ServiceLifetime.Transient)

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