using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Christofel.BaseLib.Implementations.Storages
{
    /// <summary>
    /// Thread safe storage using ImmutableArray
    /// </summary>
    /// <remarks>
    /// Useful for storages where a lot of reading is required as no synchronization
    /// mechanism is needed for reading. Writing uses regular locking, so it may become
    /// slow if used a lot from multiple threads.
    /// </remarks>
    /// <typeparam name="TData"></typeparam>
    public class ThreadSafeImmutableArrayStorage<TData> : IThreadSafeStorage<TData>
    {
        private readonly object _lock = new object();
        private ImmutableArray<TData> _data;

        public ThreadSafeImmutableArrayStorage()
        {
            _data = ImmutableArray<TData>.Empty;
        }
        
        public IReadOnlyList<TData> Data => _data;
        

        public void Add(TData data)
        {
            lock (_lock)
            {
                _data = _data.Add(data);
            }
        }
        
        public void AddRange(IEnumerable<TData> data)
        {
            lock (_lock)
            {
                _data = _data.AddRange(data);
            }
        }

        public void Remove(TData data)
        {
            lock (_lock)
            {
                _data = _data.Remove(data);
            }
        }
    }
}