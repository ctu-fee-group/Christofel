//
//   ChristofelDIPlugin.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Plugins.Runtime;
using Christofel.Remora;

namespace Christofel.BaseLib.Plugins
{
    public abstract class ChristofelDIPlugin : DIRuntimePlugin<IChristofelState, IPluginContext>
    {
        protected override IPluginContext InitializeContext() => new PluginContext();
    }
}