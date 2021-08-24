using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Discord;
using Christofel.BaseLib.Lifetime;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.Application.State
{
    public class DiscordBot : IBot, IDisposable
    {
        private readonly ILogger<DiscordBot> _logger;
        private readonly CancellationTokenSource _applicationRunningToken = new CancellationTokenSource();
        private readonly DiscordBotOptions _options;
        private readonly IApplicationLifetime _lifetime;

        public DiscordBot(DiscordSocketClient client, IOptions<DiscordBotOptions> options, ILogger<DiscordBot> logger, IApplicationLifetime lifetime)
        {
            Client = client;
            _lifetime = lifetime;
            _logger = logger;
            _options = options.Value;
        }

        public DiscordSocketClient Client { get; }

        /// <summary>
        /// Requests a quit operation
        /// </summary>
        public void QuitBot()
        {
            _applicationRunningToken.Cancel();
        }

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

        /// <summary>
        /// Logouts the bot from Discord
        /// </summary>
        /// <param name="token"></param>
        public async Task StopBot(CancellationToken token = new CancellationToken())
        {
            token.ThrowIfCancellationRequested();
            if (Client.ConnectionState == ConnectionState.Connected)
            {
                await Client.StopAsync();
                await Client.LogoutAsync();
            }
        }

        /// <summary>
        /// Starts the bot logging it to Discord
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<DiscordSocketClient> StartBotAsync(CancellationToken token = new CancellationToken())
        {
            token.ThrowIfCancellationRequested();
            _logger.LogInformation("Starting bot");

            await Client.LoginAsync(TokenType.Bot, _options.Token);
            token.ThrowIfCancellationRequested();
            await Client.StartAsync();

            return Client;
        }

        public void Dispose()
        {
            _applicationRunningToken.Dispose();
        }
    }
}