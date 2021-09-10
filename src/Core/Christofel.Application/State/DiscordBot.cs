//
//   DiscordBot.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Discord;
using Christofel.Plugins.Extensions;
using Christofel.Plugins.Lifetime;
using Microsoft.Extensions.Logging;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Results;
using Remora.Results;

namespace Christofel.Application.State
{
    /// <inheritdoc cref="Christofel.BaseLib.Discord.IBot" />
    public class DiscordBot : IBot, IDisposable
    {
        private readonly CancellationTokenSource _applicationRunningToken = new CancellationTokenSource();
        private readonly IApplicationLifetime _lifetime;
        private readonly ILogger<DiscordBot> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordBot"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The http client factory.</param>
        /// <param name="client">The gateway client.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="lifetime">The lifetime of the application.</param>
        public DiscordBot
        (
            IHttpClientFactory httpClientFactory,
            DiscordGatewayClient client,
            ILogger<DiscordBot> logger,
            IApplicationLifetime lifetime
        )
        {
            Client = client;
            HttpClientFactory = httpClientFactory;

            _lifetime = lifetime;
            _logger = logger;
        }

        /// <inheritdoc/>
        public DiscordGatewayClient Client { get; }

        /// <inheritdoc/>
        public IHttpClientFactory HttpClientFactory { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            _applicationRunningToken.Dispose();
        }

        /// <summary>
        /// Runs application in delay task until stop is requested using cancellation token.
        /// </summary>
        /// <param name="token">The cancellation token for the operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task RunApplication(CancellationToken token = default)
        {
            _logger.LogInformation("Running application");
            CancellationTokenSource tokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(_applicationRunningToken.Token, token);

            var runResult = await Client.RunAsync(token);
            if (!runResult.IsSuccess)
            {
                switch (runResult.Error)
                {
                    case ExceptionError exe:
                    {
                        _logger.LogError
                        (
                            exe.Exception,
                            "Exception during gateway connection: {ExceptionMessage}",
                            exe.Message
                        );

                        break;
                    }
                    case GatewayWebSocketError:
                    case GatewayDiscordError:
                    {
                        _logger.LogError("Gateway error: {Message}", runResult.Error.Message);
                        break;
                    }
                    default:
                    {
                        _logger.LogError("Unknown error: {Message}", runResult.Error.Message);
                        break;
                    }
                }
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
    }
}