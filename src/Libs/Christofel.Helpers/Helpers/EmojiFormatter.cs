//
//   EmojiFormatter.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Discord.API.Abstractions.Objects;

namespace Christofel.ReactHandler.Formatters
{
    /// <summary>
    /// Formatter for <see cref="IPartialEmoji"/>.
    /// </summary>
    public static class EmojiFormatter
    {
        /// <summary>
        /// Formats the emoji that can be sent to the Discord.
        /// </summary>
        /// <param name="emoji">The partial emoji to format.</param>
        /// <returns>Formatted emoji.</returns>
        public static string GetEmojiString(IPartialEmoji emoji)
        {
            string value = string.Empty;

            if (emoji.Name.IsDefined(out var name))
            {
                value = name;
            }

            if (emoji.ID.IsDefined(out var id))
            {
                return $"<:{value}:{id}>";
            }

            return value;
        }
    }
}