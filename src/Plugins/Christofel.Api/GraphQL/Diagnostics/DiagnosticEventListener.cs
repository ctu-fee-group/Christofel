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
    /// Logs all kinds of graphql errors to loggers
    /// </summary>
    public class DiagnosticEventListener : HotChocolate.Execution.Instrumentation.DiagnosticEventListener
    {
        private readonly ILogger _logger;

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
            ) ?? "";

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

        public override void RequestError(IRequestContext context, Exception exception)
        {
            _logger.LogError(exception, "Caught an exception in DiagnosticEventListener.RequestError\n");
        }

        public override void ResolverError(IMiddlewareContext context, IError error)
        {
            LogError("ResolverError", error);
        }

        public override void SyntaxError(IRequestContext context, IError error)
        {
            LogError("SyntaxError", error);
        }

        public override void ValidationErrors(IRequestContext context, IReadOnlyList<IError> errors)
        {
            LogErrors("ValidationErrors", errors);
        }

        public override void TaskError(IExecutionTask task, IError error)
        {
            LogError("TaskError", error);
        }

        public override void SubscriptionEventError(SubscriptionEventContext context, Exception exception)
        {
            _logger.LogError(exception, "Caught an exception in DiagnosticEventListener.SubscriptionEventError\n");
        }
    }
}