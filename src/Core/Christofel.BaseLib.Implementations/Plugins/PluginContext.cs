//
//   PluginContext.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Remora;

namespace Christofel.BaseLib.Plugins
{
    public class PluginContext : IPluginContext
    {
        public IAnyResponder? PluginResponder { get; set; }
    }
}