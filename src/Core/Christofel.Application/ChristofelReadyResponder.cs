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
    public class ChristofelReadyResponder : IResponder<IReady>
    {
        private readonly ChristofelApp _app;

        public ChristofelReadyResponder(ChristofelApp app)
        {
            _app = app;
        }

        public Task<Result> RespondAsync(IReady gatewayEvent, CancellationToken ct = default) =>
            // TODO: somehow move the logic here
            _app.HandleReady();
    }
}