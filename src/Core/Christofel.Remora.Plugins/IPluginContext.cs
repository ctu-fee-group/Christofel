//
//   IPluginContext.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Remora
{
    /// <summary>
    ///     Context of attached plugin
    /// </summary>
    public interface IPluginContext
    {
        /// <summary>
        ///     Responder that will be called for every event if not null
        /// </summary>
        public IAnyResponder? PluginResponder { get; }
    }
}