//
//   DetachedPlugin.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.Loader;

namespace Christofel.Plugins.Data
{
    /// <summary>
    /// Represents detached plugin with all remaining information about it.
    /// </summary>
    public class DetachedPlugin : IHasPluginInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DetachedPlugin"/> class.
        /// </summary>
        /// <param name="plugin">Attached plugin to create detached plugin from.</param>
        public DetachedPlugin(AttachedPlugin plugin)
        {
            Name = plugin.Name;
            Description = plugin.Description;
            Version = plugin.Version;
            Id = plugin.Id;
        }

        /// <summary>
        /// Gets automatically generated random Guid.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Reference to the <see cref="AssemblyLoadContext"/> the plugin was loaded inside.
        /// </summary>
        public WeakReference? AssemblyContextReference { get; internal set; }

        /// <summary>
        /// Symbols if the plugin instance was destroyed and thus can be released from memory.
        /// </summary>
        public bool Destroyed => DestroyedLate || DestroyedInTime;

        /// <summary>
        /// Symbols if the plugin was not destroyed in time, but at least was destroyed after.
        /// </summary>
        public bool DestroyedLate { get; set; }

        /// <summary>
        /// Symbols if the plugin was destroyed in time allocated (10 seconds by default).
        /// </summary>
        public bool DestroyedInTime { get; set; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string Description { get; }

        /// <inheritdoc />
        public string Version { get; }

        /// <inheritdoc />
        public override string ToString() => $@"{Name} ({Version})";
    }
}