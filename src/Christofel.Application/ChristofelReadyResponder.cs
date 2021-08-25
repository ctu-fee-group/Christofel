using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Christofel.Application
{
    public class ChristofelReadyResponder : IResponder<IReady>
    {
        private ChristofelApp _app;
        
        public ChristofelReadyResponder(ChristofelApp app)
        {
            _app = app;
        }
        
        public Task<Result> RespondAsync(IReady gatewayEvent, CancellationToken ct = new CancellationToken())
        {
            // TODO: somehow move the logic here
            _app.HandleReady();
            
            return Task.FromResult(Result.FromSuccess());
        }
    }
}