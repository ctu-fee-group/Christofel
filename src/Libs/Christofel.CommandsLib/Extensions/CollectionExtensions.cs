//
//   CollectionExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Remora.Discord.Core;

namespace Christofel.CommandsLib.Extensions
{
    /// <summary>
    /// Class containing extensions for <see cref="IReadOnlyCollection{T}"/>.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Checks if the <see cref="leftCollection"/> matches <see cref="rightCollection"/> using <see cref="matchComparer"/>.
        /// </summary>
        /// <param name="leftCollection">The collection to be matched against <see cref="rightCollection"/>.</param>
        /// <param name="rightCollection">The collection to be matched against <see cref="leftCollection"/>.</param>
        /// <param name="matchComparer">The comparer used to compare elements between the collections.</param>
        /// <typeparam name="TLeft">The type of the <see cref="leftCollection"/> collection.</typeparam>
        /// <typeparam name="TRight">The type of the <see cref="rightCollection"/> collection.</typeparam>
        /// <returns>Whether <see cref="leftCollection"/> matches <see cref="rightCollection"/>.</returns>
        public static bool CollectionMatches<TLeft, TRight>
        (
            this IReadOnlyCollection<TLeft> leftCollection,
            IReadOnlyCollection<TRight> rightCollection,
            Func<TLeft, TRight, bool> matchComparer
        )
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

        /// <summary>
        /// Checks whether the <see cref="left"/> collection has the same length as the <see cref="right"/>.
        /// </summary>
        /// <param name="left">The collection to be matched against <see cref="right"/>.</param>
        /// <param name="right">The collection to be matched against <see cref="left"/>.</param>
        /// <typeparam name="TLeft">The type of the <see cref="left"/> collection.</typeparam>
        /// <typeparam name="TRight">The type of the <see cref="right"/> collection.</typeparam>
        /// <returns>Whether lengths of both collections match.</returns>
        public static bool HasSameLength<TLeft, TRight>
        (
            this Optional<IReadOnlyList<TLeft>> left,
            Optional<IReadOnlyList<TRight>> right
        )
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

            return !left.HasValue || left.Value.Count == right.Value.Count;
        }
    }
}