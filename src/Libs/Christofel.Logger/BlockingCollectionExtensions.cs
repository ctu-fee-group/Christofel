//
//   BlockingCollectionExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Christofel.Logger
{
    /// <summary>
    /// Class containing extensions for waiting for <see cref="BlockingCollection{T}"/> to change into some state.
    /// </summary>
    public static class BlockingCollectionExtensions
    {
        /// <summary>
        /// Fetches at least one element from the collection, but multiple if available without blocking.
        /// </summary>
        /// <param name="threadSafeQueue">The blocking collection.</param>
        /// <param name="maxCount">The maximum count of items to look for.</param>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <returns>A list of all elements that were retrieved from the collection.</returns>
        public static List<T> FetchAtLeastOneBlocking<T>(this BlockingCollection<T> threadSafeQueue, uint maxCount)
        {
            var resultList = new List<T>();

            // Take() will block the thread until new elements appear
            // It will also throw an InvalidOperationException when blockingCollection is Completed
            resultList.Add(threadSafeQueue.Take());

            try
            {
                // Fetch more unblocking
                while (threadSafeQueue.Count > 0 && resultList.Count < maxCount)
                {
                    T? item;
                    var success = false;
                    success = threadSafeQueue.TryTake(out item);
                    if (success && item != null)
                    {
                        resultList.Add(item);
                    }
                }
            }
            catch (Exception)
            {
                // log.LogCritical($"Unknown error fetching more elements. Continuing to process the {resultList.Count} already fetched items.", ex);
            }

            return resultList;
        }
    }
}