//
//   HandleReactFormatter.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.ReactHandler.Database.Models;

namespace Christofel.ReactHandler.Formatters
{
    public static class HandleReactFormatter
    {
        public static string FormatHandlerTarget(HandleReact handler)
            => handler.Type switch
            {
                HandleReactType.Channel => $"<#{handler.EntityId}>",
                HandleReactType.Role => $"<@&{handler.EntityId}>",
                _ => $"invalid {handler.EntityId}",
            };
    }
}