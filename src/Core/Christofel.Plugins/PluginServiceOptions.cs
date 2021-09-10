//
//   PluginServiceOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Plugins
{
    /// <summary>
    /// Options for loading plugins.
    /// </summary>
    public class PluginServiceOptions
    {
        /// <summary>
        /// Gets folder where to look for plugins.
        /// </summary>
        public string Folder { get; set; } = null!;
    }
}