using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Discord;
using Christofel.BaseLib.Lifetime;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Discord.Caching.Services;
using Remora.Discord.Gateway;

namespace Christofel.Application.State
{
    public class DiscordBot : IBot, IDisposable
    {
        private readonly ILogger<DiscordBot> _logger;
        private readonly CancellationTokenSource _applicationRunningToken = new CancellationTokenSource();
        private readonly IApplicationLifetime _lifetime;

        public DiscordBot(IHttpClientFactory httpClientFactory, CacheService cache, DiscordGatewayClient client,
            ILogger<DiscordBot> logger, IApplicationLifetime lifetime)
        {
            Client = client;
            HttpClientFactory = httpClientFactory;
            Cache = cache;

            _lifetime = lifetime;
            _logger = logger;
        }

        public DiscordGatewayClient Client { get; }
        public CacheService Cache { get; }
        public IHttpClientFactory HttpClientFactory { get; }

        /// <summary>
        /// Runs application in delay task until stop is requested using cancellation token
        /// </summary>
        /// <param name="token"></param>
        public async Task RunApplication(CancellationToken token = new CancellationToken())
        {
            _logger.LogInformation("Running application");
            CancellationTokenSource tokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(_applicationRunningToken.Token, token);
            try
            {
                await Task.Delay(-1, tokenSource.Token);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                tokenSource.Dispose();
            }

            _lifetime.RequestStop();
            if (_lifetime.State < LifetimeState.Destroyed)
            {
                _logger.LogInformation("Going to wait for Christofel to stop. If the app hangs, just kill it");
                await _lifetime.WaitForAsync(LifetimeState.Destroyed, default);
                // Await destroyed at all costs
                // If the application is not exiting, the user can just kill it
            }
        }

        public void Dispose()
        {
            _applicationRunningToken.Dispose();
        }
    }
}