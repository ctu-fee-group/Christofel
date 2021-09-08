using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Results;

namespace Christofel.BaseLib
{
    public interface IAnyResponder
    {
        public Task<Result> RespondAsync<TEvent>(TEvent gatewayEvent, CancellationToken ct = default)
            where TEvent : IGatewayEvent;
    }
}