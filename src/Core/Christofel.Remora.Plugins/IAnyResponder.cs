//
//   IAnyResponder.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Results;

namespace Christofel.Remora
{
    /// <summary>
    /// Responds to any <see cref="IGatewayEvent"/>.
    /// </summary>
    public interface IAnyResponder
    {
        /// <summary>
        /// Responds to given event.
        /// </summary>
        /// <param name="gatewayEvent">The event to respond to.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <returns>Response result that may no have succeeded.</returns>
        public Task<Result> RespondAsync<TEvent>(TEvent gatewayEvent, CancellationToken ct = default)
            where TEvent : IGatewayEvent;
    }
}