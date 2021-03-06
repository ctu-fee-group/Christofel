//
//   DiagnosticEventListener.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Execution.Instrumentation;
using HotChocolate.Resolvers;
using Microsoft.Extensions.Logging;

namespace Christofel.Api.GraphQL.Diagnostics
{
    /// <summary>
    /// Event listener that logs all kinds of GraphQL errors to loggers.
    /// </summary>
    public class DiagnosticEventListener : HotChocolate.Execution.Instrumentation.DiagnosticEventListener
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticEventListener"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public DiagnosticEventListener(ILogger<DiagnosticEventListener> logger)
        {
            _logger = logger;
        }

        private void LogError(string method, IError error)
        {
            string path = string.Join
            (
                "/",
                error.Path?.ToString()
            );

            if (error.Exception != null)
            {
                _logger.LogError
                (
                    error.Exception,
                    $"Caught an exception in DiagnosticEventListener.{method}. Path: {path}\n"
                );
            }
            else
            {
                _logger.LogError
                    ($"Caught an error in DiagnosticEventListener.{method}. Path: {path}.\nData: {error.Message}");
            }
        }

        private void LogErrors(string method, IEnumerable<IError> errors)
        {
            foreach (IError error in errors)
            {
                LogError(method, error);
            }
        }

        /// <inheritdoc />
        public override void RequestError(IRequestContext context, Exception exception)
        {
            _logger.LogError(exception, "Caught an exception in DiagnosticEventListener.RequestError\n");
        }

        /// <inheritdoc />
        public override void ResolverError(IMiddlewareContext context, IError error)
        {
            LogError("ResolverError", error);
        }

        /// <inheritdoc />
        public override void SyntaxError(IRequestContext context, IError error)
        {
            LogError("SyntaxError", error);
        }

        /// <inheritdoc />
        public override void ValidationErrors(IRequestContext context, IReadOnlyList<IError> errors)
        {
            LogErrors("ValidationErrors", errors);
        }

        /// <inheritdoc />
        public override void TaskError(IExecutionTask task, IError error)
        {
            LogError("TaskError", error);
        }

        /// <inheritdoc />
        public override void SubscriptionEventError(SubscriptionEventContext context, Exception exception)
        {
            _logger.LogError(exception, "Caught an exception in DiagnosticEventListener.SubscriptionEventError\n");
        }
    }
}