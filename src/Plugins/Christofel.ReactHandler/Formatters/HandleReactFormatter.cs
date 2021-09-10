//
//   HandleReactFormatter.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.ReactHandler.Database.Models;

namespace Christofel.ReactHandler.Formatters
{
    /// <summary>
    /// The formatter for <see cref="HandleReact"/>.
    /// </summary>
    public static class HandleReactFormatter
    {
        /// <summary>
        /// Formats mention from the given handler.
        /// </summary>
        /// <param name="handler">The handler to format.</param>
        /// <returns>Channel or Role mention.</returns>
        public static string FormatHandlerTarget(HandleReact handler)
            => handler.Type switch
            {
                HandleReactType.Channel => $"<#{handler.EntityId}>",
                HandleReactType.Role => $"<@&{handler.EntityId}>",
                _ => $"invalid {handler.EntityId}",
            };
    }
}