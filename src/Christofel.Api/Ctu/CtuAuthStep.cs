using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Christofel.Api.Ctu
{
    public abstract class CtuAuthStep : ICtuAuthStep
    {
        protected readonly ILogger _logger;
        
        public CtuAuthStep(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Handle(CtuAuthProcessData data, Func<CtuAuthProcessData, Task> next)
        {
            data.CancellationToken.ThrowIfCancellationRequested();
            
            bool cont = false;
            string stepName = GetType()?.Name ?? "Unknown";
            using (_logger.BeginScope($"CTU auth step {stepName}"))
            {
                _logger.LogDebug($"Starting auth step step {stepName}");
                cont = await HandleStep(data);
                _logger.LogDebug($"Auth step {stepName} successfully returned");
            }

            if (cont)
            {
                await next(data);
                await HandleAfterNext(data);
            }
        }

        protected abstract Task<bool> HandleStep(CtuAuthProcessData data);
        
        protected virtual Task HandleAfterNext(CtuAuthProcessData data)
        {
            return Task.CompletedTask;
        }
    }
}