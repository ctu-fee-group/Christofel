//
//   ThreadSafeListStorage.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Christofel.BaseLib.Implementations.Storages
{
    /// <summary>
    /// Thread safe storage using <see cref="List{T}"/>.
    /// </summary>
    /// <remarks>
    /// Useful for storages where there is not a lot of reading as well as not a lot of writing.
    /// Uses locking for both reading and writing.
    /// Use this in favor of <see cref="ThreadSafeImmutableArrayStorage{TData}" />
    /// if you do not need a lot of WR and want to save some space.
    /// </remarks>
    /// <typeparam name="TData">The type of the data.</typeparam>
    public class ThreadSafeListStorage<TData> : IThreadSafeStorage<TData>
    {
        private readonly List<TData> _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadSafeListStorage{TData}"/> class.
        /// </summary>
        public ThreadSafeListStorage()
        {
            _data = new List<TData>();
        }

        /// <inheritdoc />
        public IReadOnlyList<TData> Data
        {
            get
            {
                lock (_data)
                {
                    return new List<TData>(_data);
                }
            }
        }

        /// <inheritdoc />
        public void Add(TData data)
        {
            lock (_data)
            {
                _data.Add(data);
            }
        }

        /// <inheritdoc />
        public void AddRange(IEnumerable<TData> data)
        {
            lock (_data)
            {
                _data.AddRange(data);
            }
        }

        /// <inheritdoc />
        public void Remove(TData data)
        {
            lock (_data)
            {
                _data.Remove(data);
            }
        }
    }
}