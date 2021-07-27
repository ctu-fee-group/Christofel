using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Discord;
using Discord;
using Discord.WebSocket;

namespace Christofel.Application
{
    public class DiscordBot : IBot
    {
        private DiscordSocketClient? _client;
        private IReadableConfig _config;
        private CancellationTokenSource _applicationRunningToken = new CancellationTokenSource();

        public DiscordBot(IReadableConfig config)
        {
            _config = config;
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
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                AlwaysAcknowledgeInteractions = false
            });

            await _client.LoginAsync(TokenType.Bot, await _config.GetAsync<string>("discord.bot.token"));
            await _client.StartAsync();

            return _client;
        }
    }
}