using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Christofel.Application.Extensions
{
    public static class BlockingCollectionExtensions
    {
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
                    bool success = false;
                    success = threadSafeQueue.TryTake(out item);
                    if (success && item != null)
                    {
                        resultList.Add(item);
                    }
                    else
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                //log.LogCritical($"Unknown error fetching more elements. Continuing to process the {resultList.Count} already fetched items.", ex);
            }

            return resultList;
        }
    }
}