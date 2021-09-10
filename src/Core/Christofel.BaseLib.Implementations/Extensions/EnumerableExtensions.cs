//
//   EnumerableExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Christofel.BaseLib.Extensions
{
    /// <summary>
    /// Class containing extensions for <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Chunks the string into string of maximal length chunkSize.
        /// </summary>
        /// <param name="str">The string to be chunked.</param>
        /// <param name="chunkSize">The size of one chunk.</param>
        /// <returns>Chunked enumerable of the string.</returns>
        public static IEnumerable<string> Chunk(this string? str, int chunkSize) =>
            !string.IsNullOrEmpty(str)
                ? Enumerable.Range(0, (int)Math.Ceiling((double)str.Length / chunkSize))
                    .Select
                    (
                        i => str
                            .Substring
                            (
                                i * chunkSize,
                                (i * chunkSize) + chunkSize <= str.Length ? chunkSize : str.Length - (i * chunkSize)
                            )
                    )
                : Enumerable.Empty<string>();
    }
}