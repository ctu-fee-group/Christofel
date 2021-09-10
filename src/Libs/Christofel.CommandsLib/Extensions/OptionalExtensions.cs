//
//   OptionalExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Discord.Core;

namespace Christofel.CommandsLib.Extensions
{
    /// <summary>
    /// Class containing extensions for <see cref="Optional{T}"/>.
    /// </summary>
    public static class OptionalExtensions
    {
        /// <summary>
        /// Checks whether optional value of <paramref name="left"/> booleans matches <paramref name="right"/>>.
        /// </summary>
        /// <param name="left">The value to be matched against <paramref name="right"/>.</param>
        /// <param name="right">The value to be matched against <paramref name="left"/>.</param>
        /// <param name="default">Default value in case of any of the values is Empty.</param>
        /// <returns>Whether <paramref name="left"/> matches <paramref name="right"/>.</returns>
        public static bool CheckOptionalBoolMatches
            (this Optional<bool> left, Optional<bool> right, bool @default) => (left.HasValue
            ? left.Value
            : @default) == (right.HasValue
            ? right.Value
            : @default);
    }
}