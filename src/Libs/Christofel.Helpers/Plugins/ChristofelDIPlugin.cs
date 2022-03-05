//
//   ChristofelDIPlugin.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Common;
using Christofel.Plugins.Runtime;
using Christofel.Remora;

namespace Christofel.BaseLib.Plugins
{
    /// <summary>
    /// Runtime plugin driven by Microsoft.Extensions.DependencyInjection for Christofel.
    /// </summary>
    /// <remarks>
    /// Implements handling of lifetime for a plugin,
    /// allowing the user to implement
    /// how services should be created.
    /// </remarks>
    public abstract class ChristofelDIPlugin : DIRuntimePlugin<IChristofelState, PluginContext>
    {
        /// <inheritdoc />
        protected override PluginContext InitializeContext() => new PluginContext();
    }
}