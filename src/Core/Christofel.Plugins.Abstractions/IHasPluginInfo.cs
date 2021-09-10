//
//   IHasPluginInfo.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Plugins
{
    /// <summary>
    /// Holds information about a plugin.
    /// </summary>
    public interface IHasPluginInfo
    {
        /// <summary>
        /// Gets the name of the plugin.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of the plugin.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the version of the plugin.
        /// </summary>
        public string Version { get; }
    }
}