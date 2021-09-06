using System;
using Christofel.ReactHandler.Database.Models;

namespace Christofel.ReactHandler.Formatters
{
    public static class HandleReactFormatter
    {
        public static string FormatHandlerTarget(HandleReact handler)
            => handler.Type switch
            {
                HandleReactType.Channel => $"{handler.Emoji} - <#{handler.EntityId}>",
                HandleReactType.Role => $"{handler.Emoji} - <@&{handler.EntityId}>",
                _ => throw new InvalidOperationException("Invalid handler type")
            };
    }
}