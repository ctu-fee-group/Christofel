//
//   PluginJobsRepository.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Christofel.Helpers.Scheduler
{
    /// <summary>
    /// Holds types of jobs that should be handled by the current plugin.
    /// </summary>
    public class PluginJobsRepository
    {
        private readonly HashSet<Type> _registeredTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginJobsRepository"/> class.
        /// </summary>
        public PluginJobsRepository()
        {
            _registeredTypes = new HashSet<Type>();
        }

        /// <summary>
        /// Registers the given type.
        /// </summary>
        /// <param name="type">The type to register.</param>
        public void RegisterType(Type type)
        {
            _registeredTypes.Add(type);
        }

        /// <summary>
        /// Gets whether the specified type is registered.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>Whether the specified types is registered.</returns>
        public bool ContainsType(Type type) => _registeredTypes.Contains(type);
    }
}