//
//  OperationContextExtensions.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;

namespace Christofel.BaseLib.Extensions;

/// <summary>
/// Extension methods for <see cref="IOperationContext"/>.
/// </summary>
public static class OperationContextExtensions
{
    /// <summary>
    /// Tries to get "username#denominator" from the context.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <param name="discordHandle">The discord handle (username#denominator), not null when true returned.</param>
    /// <returns>Whether discord handle has been found and filled.</returns>
    public static bool TryGetUserDiscordHandle(this IOperationContext context, [NotNullWhen(true)] out string? discordHandle)
    {
        discordHandle = null;

        switch (context)
        {
            case IInteractionContext interactionCommandContext:
            {
                if (interactionCommandContext.Interaction.User.TryGet(out var user))
                {
                    discordHandle = $"{user.Username}#{user.Discriminator}";
                    return true;
                }

                if (interactionCommandContext.Interaction.Member.TryGet(out var member))
                {
                    if (member.User.TryGet(out user))
                    {
                        discordHandle = $"{user.Username}#{user.Discriminator}";
                        return true;
                    }
                }

                break;
            }
            case IMessageContext { Message.Author.HasValue: true } messageContext:
            {
                var user = messageContext.Message.Author.Value;
                discordHandle = $"{user.Username}#{user.Discriminator}";
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Tries to get user discord handle from context (username#denominator), in case it is not found, <paramref name="@default"/> will be returned.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <param name="default">The default value in case the handle is not found.</param>
    /// <returns>The discord handle, or default if not found.</returns>
    public static string GetUserDiscordHandleOrDefault(this IOperationContext context, string @default = "Unknown")
    {
        if (context.TryGetUserDiscordHandle(out var discordHandle))
        {
            return discordHandle;
        }

        return @default;
    }
}