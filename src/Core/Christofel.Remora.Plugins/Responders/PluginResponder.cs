//
//   PluginResponder.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Discord.Gateway.Services;
using Remora.Results;

namespace Christofel.Remora.Responders
{
    /// <summary>
    /// <see cref="IAnyResponder"/> for plugins that will call event responders
    /// from <see cref="IResponderTypeRepository"/> that were registered.
    /// </summary>
    public class PluginResponder : IAnyResponder
    {
        private readonly ILogger _logger;
        private readonly IResultLoggerProvider? _resultLoggerProvider;
        private readonly IResponderTypeRepository _responderTypeRepository;
        private readonly IServiceProvider _services;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginResponder"/> class.
        /// </summary>
        /// <param name="responderTypeRepository">The repository of responder types.</param>
        /// <param name="services">The service provider for resolving event resolvers.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="resultLoggerProvider">The result logger provider.</param>
        public PluginResponder
        (
            IResponderTypeRepository responderTypeRepository,
            IServiceProvider services,
            ILogger<PluginResponder> logger,
            IResultLoggerProvider? resultLoggerProvider = default
        )
        {
            _responderTypeRepository = responderTypeRepository;
            _services = services;
            _logger = logger;
            _resultLoggerProvider = resultLoggerProvider;
        }

        /// <inheritdoc />
        public async Task<Result> RespondAsync<TEvent>(TEvent gatewayEvent, CancellationToken ct = default)
            where TEvent : IGatewayEvent
        {
            var responderGroups = new[]
            {
                _responderTypeRepository.GetEarlyResponderTypes<TEvent>(),
                _responderTypeRepository.GetResponderTypes<TEvent>(),
                _responderTypeRepository.GetLateResponderTypes<TEvent>(),
            };

            foreach (var responderGroup in responderGroups)
            {
                await Task.WhenAll
                (
                    responderGroup.Select
                        (
                            async rt =>
                            {
                                using var serviceScope = _services.CreateScope();
                                var responder = (IResponder<TEvent>)serviceScope.ServiceProvider
                                    .GetRequiredService(rt);

                                try
                                {
                                    return await responder.RespondAsync(gatewayEvent, ct);
                                }
                                catch (Exception e)
                                {
                                    return e;
                                }
                            }
                        )
                        .Select(HandleEventResult)
                ).ConfigureAwait(false);
            }

            return Result.FromSuccess(); // Everything error handled and logged
        }

        /// <summary>
        /// Handles the result of event.
        /// </summary>
        /// <param name="eventDispatch">The asynchronous task that represents the event.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        private async Task HandleEventResult(Task<Result> eventDispatch)
        {
            var responderResult = await eventDispatch;

            if (responderResult.IsSuccess)
            {
                return;
            }

            LogResult(responderResult);
        }

        /// <summary>
        /// Logs a result based on the type of the error.
        /// </summary>
        /// <param name="result">The result to be logged.</param>
        private void LogResult(IResult result)
        {
            if (result.IsSuccess)
            {
                return;
            }

            if (_resultLoggerProvider is not null)
            {
                _resultLoggerProvider.Log(_logger, result, "An error has occured from PluginResponder call.");
                return;
            }

            switch (result.Error)
            {
                case ExceptionError exe:
                {
                    _logger.LogWarning
                    (
                        exe.Exception,
                        "Error in plugin responder event responder: {Exception}",
                        exe.Message
                    );

                    break;
                }
                case AggregateError aggregateError:
                {
                    foreach (var errorResult in aggregateError.Errors)
                    {
                        LogResult(errorResult);
                    }

                    break;
                }
                default:
                {
                    _logger.LogWarning
                    (
                        "Error in plugin responder event responder.\n{Reason}",
                        result.Error?.Message
                    );

                    break;
                }
            }
        }
    }
}