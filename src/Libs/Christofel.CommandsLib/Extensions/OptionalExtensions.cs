//
//   OptionalExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Discord.Core;

namespace Christofel.CommandsLib.Extensions
{
    public static class OptionalExtensions
    {
        public static bool CheckOptionalBoolMatches
            (this Optional<bool> left, Optional<bool> right, bool @default) => (left.HasValue
            ? left.Value
            : @default) == (right.HasValue
            ? right.Value
            : @default);
    }
}