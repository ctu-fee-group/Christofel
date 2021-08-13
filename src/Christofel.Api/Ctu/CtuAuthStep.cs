using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Christofel.Api.Ctu
{
    /// <summary>
    /// Class to ease working with ctu auth steps
    /// </summary>
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
        
        /// <summary>
        /// Handle step before next is called
        /// </summary>
        /// <param name="data"></param>
        /// <returns>If true, next will be called, if false, next won't be called and the process will end</returns>
        protected abstract Task<bool> HandleStep(CtuAuthProcessData data);
        
        /// <summary>
        /// Handling after next, that means after the user was authenticated
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected virtual Task HandleAfterNext(CtuAuthProcessData data)
        {
            return Task.CompletedTask;
        }
    }
}