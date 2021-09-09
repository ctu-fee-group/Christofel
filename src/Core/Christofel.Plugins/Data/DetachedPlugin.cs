//
//   DetachedPlugin.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Christofel.Plugins.Data
{
    /// <summary>
    ///     Holding state of detached plugin
    /// </summary>
    public class DetachedPlugin : IHasPluginInfo
    {
        public DetachedPlugin(AttachedPlugin plugin)
        {
            Name = plugin.Name;
            Description = plugin.Description;
            Version = plugin.Version;
            Id = plugin.Id;
        }

        public Guid Id { get; }

        public WeakReference? AssemblyContextReference { get; internal set; }

        /// <summary>
        ///     Symbols if the plugin instance was destroyed and thus can be released from memory
        /// </summary>
        public bool Destroyed => DestroyedLate || DestroyedInTime;

        /// <summary>
        ///     Symbols if the plugin was not destroyed in time, but at least was destroyed after
        /// </summary>
        public bool DestroyedLate { get; set; }

        /// <summary>
        ///     Symbols if the plugin was destroyed in time allocated (10 seconds by default)
        /// </summary>
        public bool DestroyedInTime { get; set; }

        public string Name { get; }
        public string Description { get; }
        public string Version { get; }

        public override string ToString() => $@"{Name} ({Version})";
    }
}