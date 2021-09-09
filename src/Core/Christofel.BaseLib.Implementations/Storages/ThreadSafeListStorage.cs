//
//   ThreadSafeListStorage.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Christofel.BaseLib.Implementations.Storages
{
    /// <summary>
    /// </summary>
    /// <remarks>
    ///     Useful for storages where there is not a lot of reading as well as not a lot of writing.
    ///     Uses locking for both reading and writing.
    ///     Use this in favor of <see cref="ThreadSafeImmutableArrayStorage{TData}" />
    ///     if you do not need a lot of WR and want to save some space.
    /// </remarks>
    /// <typeparam name="TData"></typeparam>
    public class ThreadSafeListStorage<TData> : IThreadSafeStorage<TData>
    {
        private readonly List<TData> _data;

        public ThreadSafeListStorage()
        {
            _data = new List<TData>();
        }

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

        public void Add(TData data)
        {
            lock (_data)
            {
                _data.Add(data);
            }
        }

        public void AddRange(IEnumerable<TData> data)
        {
            lock (_data)
            {
                _data.AddRange(data);
            }
        }

        public void Remove(TData data)
        {
            lock (_data)
            {
                _data.Remove(data);
            }
        }
    }
}