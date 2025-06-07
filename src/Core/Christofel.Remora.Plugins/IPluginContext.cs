//
//   IPluginContext.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Discord.API.Abstractions.Gateway.Events;

namespace Christofel.Remora
{
    /// <summary>
    /// Context of attached plugin holding a responder to <see cref="IGatewayEvent"/>.
    /// </summary>
    public interface IPluginContext
    {
        /// <summary>
        /// Gets responder that will be called for every <see cref="IGatewayEvent"/> if it is not null.
        /// </summary>
        public IAnyResponder? PluginResponder { get; }
    }
}