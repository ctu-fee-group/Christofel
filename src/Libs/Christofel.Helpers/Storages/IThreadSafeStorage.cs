//
//   IThreadSafeStorage.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Christofel.Helpers.Storages
{
    /// <summary>
    /// Exposing thread safe storage that can be used with a list-like interface.
    /// </summary>
    /// <typeparam name="TData">The data type that will be stored.</typeparam>
    public interface IThreadSafeStorage<TData>
    {
        /// <summary>
        /// Gets all currently stored data in the storage.
        /// </summary>
        public IReadOnlyList<TData> Data { get; }

        /// <summary>
        /// Add data to the storage thread-safely.
        /// </summary>
        /// <param name="data">The data to be added.</param>
        public void Add(TData data);

        /// <summary>
        /// Add multiple data thread-safely.
        /// </summary>
        /// <param name="data">The data to be added.</param>
        public void AddRange(IEnumerable<TData> data);

        /// <summary>
        /// Remove matching entity from the storage.
        /// </summary>
        /// <param name="data">The data to be removed.</param>
        public void Remove(TData data);
    }
}