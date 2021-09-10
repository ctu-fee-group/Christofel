//
//   PluginAutoloaderOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Application.Plugins
{
    /// <summary>
    /// Options for <see cref="PluginAutoloader"/>.
    /// </summary>
    public class PluginAutoloaderOptions
    {
        /// <summary>
        /// Gets or sets plugins to autoload on startup.
        /// </summary>
        public string[]? AutoLoad { get; set; }
    }
}