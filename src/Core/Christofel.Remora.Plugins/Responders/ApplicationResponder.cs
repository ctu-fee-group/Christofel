//
//   ApplicationResponder.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Plugins;
using Christofel.Remora;
using Christofel.Remora.Responders;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.Application.Responders
{
    /// <summary>
    /// <see cref="IEveryResponder"/> for an application holding runtime plugins that will call responders of each plugin.
    /// </summary>
    /// <typeparam name="TState">The state of the application.</typeparam>
    /// <typeparam name="TContext">The context of the plugins.</typeparam>
    public class ApplicationResponder<TState, TContext> : EveryResponder
        where TContext : IPluginContext
    {
        private readonly ILogger _logger;
        private readonly IResultLoggerProvider? _resultLoggerProvider;
        private readonly PluginStorage _plugins;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationResponder{TState, TContext}"/> class.
        /// </summary>
        /// <param name="plugins">The storage of the plugins.</param>
        /// <param name="logger">The logger to log state into.</param>
        /// <param name="resultLoggerProvider">The result logger provider.</param>
        public ApplicationResponder
        (
            PluginStorage plugins,
            ILogger<ApplicationResponder<TState, TContext>> logger,
            IResultLoggerProvider? resultLoggerProvider = default
        )
        {
            _plugins = plugins;
            _logger = logger;
            _resultLoggerProvider = resultLoggerProvider;
        }

        /// <inheritdoc />
        public override async Task<Result> RespondAnyAsync<TEvent>(TEvent gatewayEvent, CancellationToken ct = default)
        {
            await Task.WhenAll
            (
                _plugins.AttachedPlugins
                    .Select(x => x.Plugin)
                    .OfType<IRuntimePlugin<TState, TContext>>()
                    .Select(x => x.Context.PluginResponder)
                    .Where(x => x is not null)
                    .Cast<IAnyResponder>()
                    .Select
                    (
                        async responder =>
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
                        }
                    )
                    .Select(HandleEventResult)
            );

            return Result.FromSuccess(); // Everything is handled in HandleEventResult
        }

        /// <summary>
        /// Handles result of the event.
        /// </summary>
        /// <param name="eventDispatch">The task representing the asynchronous event.</param>
        private async Task HandleEventResult(Task<Result> eventDispatch)
        {
            var responderResult = await eventDispatch;
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
                _resultLoggerProvider.Log(_logger, result, "An error has occured from ApplicationResponder call.");
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