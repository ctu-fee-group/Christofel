//
//   ThreadSafeImmutableArrayStorage.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;

namespace Christofel.BaseLib.Implementations.Storages
{
    /// <summary>
    /// Thread safe storage using <see cref="ImmutableArray{T}"/>.
    /// </summary>
    /// <remarks>
    /// Useful for storages where a lot of reading is required as no synchronization
    /// mechanism is needed for reading. Writing uses regular locking, so it may become
    /// slow if used a lot from multiple threads.
    /// </remarks>
    /// <typeparam name="TData">The type of the data.</typeparam>
    public class ThreadSafeImmutableArrayStorage<TData> : IThreadSafeStorage<TData>
    {
        private readonly object _lock = new object();
        private ImmutableArray<TData> _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadSafeImmutableArrayStorage{TData}"/> class.
        /// </summary>
        public ThreadSafeImmutableArrayStorage()
        {
            _data = ImmutableArray<TData>.Empty;
        }

        /// <inheritdoc />
        public IReadOnlyList<TData> Data => _data;

        /// <inheritdoc />
        public void Add(TData data)
        {
            lock (_lock)
            {
                _data = _data.Add(data);
            }
        }

        /// <inheritdoc />
        public void AddRange(IEnumerable<TData> data)
        {
            lock (_lock)
            {
                _data = _data.AddRange(data);
            }
        }

        /// <inheritdoc />
        public void Remove(TData data)
        {
            lock (_lock)
            {
                _data = _data.Remove(data);
            }
        }
    }
}