using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Discord;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.Application.State
{
    public class DiscordBot : IBot
    {
        private ILogger<DiscordBot> _logger;
        private CancellationTokenSource _applicationRunningToken = new CancellationTokenSource();
        private DiscordBotOptions _options;

        public DiscordBot(DiscordSocketClient client, IOptions<DiscordBotOptions> options, ILogger<DiscordBot> logger)
        {
            Client = client;
            _logger = logger;
            _options = options.Value;
        }

        public DiscordSocketClient Client { get; }

        public void QuitBot()
        {
            _applicationRunningToken.Cancel();
        }

        public async Task RunApplication(CancellationToken token = new CancellationToken())
        {
            _logger.LogInformation("Running application");
            try
            {
                CancellationTokenSource tokenSource = 
                    CancellationTokenSource.CreateLinkedTokenSource(_applicationRunningToken.Token, token);
                await Task.Delay(-1, tokenSource.Token);
            }
            catch (TaskCanceledException) {}
        }

        public async Task StopBot(CancellationToken token = new CancellationToken())
        {
            token.ThrowIfCancellationRequested();
            if (Client.ConnectionState == ConnectionState.Connected)
            {
                await Client.StopAsync();
                await Client.LogoutAsync();
            }
        }

        public async Task<DiscordSocketClient> StartBotAsync(CancellationToken token = new CancellationToken())
        {
            token.ThrowIfCancellationRequested();
            _logger.LogInformation("Starting bot");

            await Client.LoginAsync(TokenType.Bot, _options.Token);
            token.ThrowIfCancellationRequested();
            await Client.StartAsync();

            return Client;
        }
    }
}