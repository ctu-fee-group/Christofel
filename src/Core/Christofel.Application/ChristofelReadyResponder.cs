//
//   ChristofelReadyResponder.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Christofel.Application
{
    /// <summary>
    /// Responder to <see cref="IReady"/> event that will start Christofel services.
    /// </summary>
    public class ChristofelReadyResponder : IResponder<IReady>
    {
        private readonly ChristofelApp _app;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChristofelReadyResponder"/> class.
        /// </summary>
        /// <param name="app">The application.</param>
        public ChristofelReadyResponder(ChristofelApp app)
        {
            _app = app;
        }

        /// <inheritdoc/>
        public Task<Result> RespondAsync(IReady gatewayEvent, CancellationToken ct = default) =>
            _app.HandleReady();
    }
}