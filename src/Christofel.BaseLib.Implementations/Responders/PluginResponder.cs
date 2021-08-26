using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Discord.Gateway.Services;
using Remora.Results;

namespace Christofel.BaseLib.Implementations.Responders
{
    public class PluginResponder : IAnyResponder
    {
        private readonly IResponderTypeRepository _responderTypeRepository;
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;

        public PluginResponder(IResponderTypeRepository responderTypeRepository, IServiceProvider services,
            ILogger<PluginResponder> logger)
        {
            _responderTypeRepository = responderTypeRepository;
            _services = services;
            _logger = logger;
        }

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
                            finally
                            {
                                // Suspicious type conversions are disabled here, since the user-defined responders may
                                // implement IDisposable or IAsyncDisposable.

                                // ReSharper disable once SuspiciousTypeConversion.Global
                                if (responder is IDisposable disposable)
                                {
                                    disposable.Dispose();
                                }

                                // ReSharper disable once SuspiciousTypeConversion.Global
                                if (responder is IAsyncDisposable asyncDisposable)
                                {
                                    await asyncDisposable.DisposeAsync();
                                }
                            }
                        }
                    )
                        .Select(HandleEventResult)
                ).ConfigureAwait(false);
            }

            return Result.FromSuccess(); // Everything error handled and logged
        }
        
        private async Task HandleEventResult(Task<Result> eventDispatch)
        {
            var responderResult = await eventDispatch;

            if (responderResult.IsSuccess)
            {
                return;
            }

            switch (responderResult.Error)
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
                default:
                {
                    _logger.LogWarning
                    (
                        "Error in plugin responder event responder.\n{Reason}",
                        responderResult.Error.Message
                    );

                    break;
                }
            }
        }
    }
}