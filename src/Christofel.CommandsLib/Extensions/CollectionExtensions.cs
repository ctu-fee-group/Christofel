using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Remora.Discord.Core;

namespace Christofel.CommandsLib.Extensions
{
    public static class CollectionExtensions
    {
        public static bool CollectionMatches<TLeft, TRight>(this IReadOnlyCollection<TLeft> leftCollection, IReadOnlyCollection<TRight> rightCollection, Func<TLeft, TRight, bool> matchComparer)
        {
            if (leftCollection.Count != rightCollection.Count)
            {
                return false;
            }

            using var leftEnumerator = leftCollection.GetEnumerator();
            using var rightEnumerator = rightCollection.GetEnumerator();

            while (leftEnumerator.MoveNext() && rightEnumerator.MoveNext())
            {
                var left = leftEnumerator.Current;
                var right = rightEnumerator.Current;

                if (!matchComparer(left, right))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool HasSameLength<TLeft, TRight>(this Optional<IReadOnlyList<TLeft>> left,
            Optional<IReadOnlyList<TRight>> right)
        {
            if ((!left.HasValue && right.HasValue && right.Value.Count == 0) ||
                (!right.HasValue && left.HasValue && left.Value.Count == 0))
            {
                return true;
            }

            if (left.HasValue != right.HasValue)
            {
                return false;
            }

            return !left.HasValue || (left.Value.Count == right.Value.Count);
        }
    }
}