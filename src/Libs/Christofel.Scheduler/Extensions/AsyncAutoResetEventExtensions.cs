//
//   AsyncAutoResetEventExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace Christofel.Scheduler.Extensions
{
    /// <summary>
    /// Defines extension methods for the type <see cref="AsyncAutoResetEvent"/>.
    /// </summary>
    public static class AsyncAutoResetEventExtensions
    {
        /// <summary>
        /// Asynchronously waits for this event to be set. If the event is set, this method will auto-reset it and return immediately, even if the cancellation token is already signalled. If the wait is canceled, then it will not auto-reset this event.
        /// </summary>
        /// <param name="resetEvent">The event to be waited.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel this wait.</param>
        /// <returns>True if the event was set, false if the <paramref name="cancellationToken"/> was canceled.</returns>
        public static async Task<bool> WaitSafeAsync
            (this AsyncAutoResetEvent resetEvent, CancellationToken cancellationToken = default)
        {
            try
            {
                await resetEvent.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return false;
            }

            return true;
        }
    }
}