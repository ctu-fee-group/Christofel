//
//   IHasPluginInfo.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Plugins
{
    /// <summary>
    ///     Abstraction of plugin info to be able to use info about plugin even after plugin was destroyed
    /// </summary>
    public interface IHasPluginInfo
    {
        public string Name { get; }
        public string Description { get; }
        public string Version { get; }
    }
}