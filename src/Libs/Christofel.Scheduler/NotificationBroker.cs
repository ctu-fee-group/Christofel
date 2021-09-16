//
//  NotificationBroker.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace Christofel.Scheduler
{
    /// <summary>
    /// Broker for notifications.
    /// </summary>
    /// <typeparam name="T">The type of the objects for the notification object.</typeparam>
    public class NotificationBroker<T>
    {
        private readonly AsyncLock _lock;
        private readonly AsyncAutoResetEvent _resetEvent;
        private Queue<T> _notificationEvents;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationBroker{T}"/> class.
        /// </summary>
        /// <param name="resetEvent">The event to be set on notification received.</param>
        public NotificationBroker(AsyncAutoResetEvent resetEvent)
        {
            _notificationEvents = new Queue<T>();
            _lock = new AsyncLock();
            _resetEvent = resetEvent;
        }

        /// <summary>
        /// Notifies about the specified data.
        /// </summary>
        /// <param name="data">The data to notify about.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task NotifyAsync(T data)
        {
            using (await _lock.LockAsync())
            {
                _notificationEvents.Enqueue(data);
            }
            _resetEvent.Set();
        }

        /// <summary>
        /// Returns whether there are some pending notifications.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task<bool> HasPendingNotifications()
        {
            using (await _lock.LockAsync())
            {
                return _notificationEvents.Count > 0;
            }
        }

        /// <summary>
        /// Gets notifications along with a lock.
        /// </summary>
        /// <remarks>
        /// The lock should be disposed after operation on the data is done.
        /// </remarks>
        /// <returns>The notifications with obtained lock.</returns>
        public async Task<(IDisposable DisposableLock, Queue<T> Notifications)> GetNotifications() => (await _lock.LockAsync(), _notificationEvents);
    }
}