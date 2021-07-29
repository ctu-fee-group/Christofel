using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Discord;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Christofel.Application.State
{
    public class DiscordBot : IBot
    {
        private DiscordSocketClient? _client;
        private ILogger<DiscordBot> _logger;
        private CancellationTokenSource _applicationRunningToken = new CancellationTokenSource();
        private DiscordBotOptions _options;

        public DiscordBot(DiscordBotOptions options, ILogger<DiscordBot> logger)
        {
            _logger = logger;
            _options = options;
        }

        public DiscordSocketClient Client
        {
            get
            {
                if (_client == null)
                {
                    throw new InvalidOperationException("Client is not initialized");
                }

                return _client;
            }
        }

        public void QuitBot()
        {
            _applicationRunningToken.Cancel();
        }

        public async Task RunApplication()
        {
            _logger.LogInformation("Running application");
            try
            {
                await Task.Delay(-1, _applicationRunningToken.Token);
            }
            catch (TaskCanceledException) {}
        }

        public async Task StopBot()
        {
            if (Client.ConnectionState == ConnectionState.Connected)
            {
                await Client.StopAsync();
                await Client.LogoutAsync();
            }
        }

        public async Task<DiscordSocketClient> StartBotAsync()
        {
            _logger.LogInformation("Starting bot");
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                AlwaysAcknowledgeInteractions = false
            });

            await _client.LoginAsync(TokenType.Bot, _options.Token);
            await _client.StartAsync();

            return _client;
        }
    }
}