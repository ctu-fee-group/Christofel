using System;
using System.Collections.Generic;
using System.Linq;

namespace Christofel.BaseLib.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<string> Chunk(this string? str, int chunkSize) =>
            !string.IsNullOrEmpty(str) ?
                Enumerable.Range(0, (int)Math.Ceiling(((double)str.Length) / chunkSize))
                    .Select(i => str
                        .Substring(i * chunkSize,
                            (i * chunkSize + chunkSize <= str.Length) ? chunkSize : str.Length - i * chunkSize))
                : Enumerable.Empty<string>();
    }
}