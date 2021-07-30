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

            await Client.LoginAsync(TokenType.Bot, _options.Token);
            await Client.StartAsync();

            return Client;
        }
    }
}