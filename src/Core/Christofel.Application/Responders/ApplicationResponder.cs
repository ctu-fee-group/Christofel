using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Application.Plugins;
using Christofel.BaseLib;
using Christofel.BaseLib.Implementations.Responders;
using Christofel.BaseLib.Plugins;
using Christofel.Plugins;
using Christofel.Plugins.Data;
using Microsoft.Extensions.Logging;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Christofel.Application.Responders
{
    public class ApplicationResponder : EveryResponder
    {
        private PluginStorage _plugins;
        private ILogger _logger;

        public ApplicationResponder(PluginStorage plugins, ILogger<ApplicationResponder> logger)
        {
            _plugins = plugins;
            _logger = logger;
        }

        public override async Task<Result> RespondAnyAsync<TEvent>(TEvent gatewayEvent, CancellationToken ct = default)
        {
            await Task.WhenAll(
                _plugins.AttachedPlugins
                    .Select(x => x.Plugin)
                    .OfType<IRuntimePlugin<IChristofelState, IPluginContext>>()
                    .Select(x => x.Context.PluginResponder)
                    .Where(x => x is not null)
                    .Cast<IAnyResponder>()
                    .Select(async (responder) =>
                    {
                        try
                        {
                            // Cannot trust the responder that there won't be an exception thrown
                            return await responder.RespondAsync(gatewayEvent, ct);
                        }
                        catch (Exception e)
                        {
                            return (Result)e;
                        }
                    })
                    .Select(HandleEventResult));

            return Result.FromSuccess(); // Everything is handled in HandleEventResult
        }

        private async Task HandleEventResult(Task<Result> eventDispatch)
        {
            var responderResult = await eventDispatch;
            LogResult(responderResult);
        }

        private void LogResult(IResult result)
        {
            if (result.IsSuccess)
            {
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