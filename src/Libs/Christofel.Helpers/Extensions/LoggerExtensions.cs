//
//   LoggerExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.IO;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.BaseLib.Extensions;

/// <summary>
/// Extension methods for <see cref="ILogger"/>.
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// Logs the given result if it isn't successful.
    /// </summary>
    /// <param name="logger">The logger to log with.</param>
    /// <param name="result">The result to log.</param>
    /// <param name="message">The message to prepend.</param>
    /// <exception cref="InvalidOperationException">Thrown if the result was successful.</exception>
    public static void LogResultError(this ILogger logger, IResult result, string message = "Encountered an error")
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("The result was successful, only unsuccessful results are supported.");
        }

        using var stringWriter = new StringWriter();
        using var logTextWriter = new IndentedTextWriter(stringWriter, " ");
        logTextWriter.WriteLine(message);
        logTextWriter.Indent++;

        LogResultError(logTextWriter, result);
        logger.LogError(stringWriter.ToString());
    }

    /// <summary>
    /// Gets the full string.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <returns>The final string.</returns>
    public static string ToFullString(this IResult result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("The result was successful, only unsuccessful results are supported.");
        }

        using var stringWriter = new StringWriter();
        using var logTextWriter = new IndentedTextWriter(stringWriter, " ");
        logTextWriter.WriteLine("Encountered an error");
        logTextWriter.Indent++;

        LogResultError(logTextWriter, result);
        return stringWriter.ToString();
    }

    private static void LogResultError(IndentedTextWriter logTextWriter, IResult result)
    {
        switch (result.Error)
        {
            case AggregateError aggregateError:
                LogAggreggateError(logTextWriter, aggregateError);
                break;
            default:
                LogNestedError(logTextWriter, result);
                break;
        }
    }

    private static void LogAggreggateError(IndentedTextWriter logTextWriter, AggregateError error)
    {
        foreach (var result in error.Errors)
        {
            LogResultError(logTextWriter, result);
        }
    }

    private static void LogNestedError(IndentedTextWriter logTextWriter, IResult? result)
    {
        if ((result?.IsSuccess ?? true) || result.Error is null)
        {
            throw new InvalidOperationException("The result was successful, it has to be unsuccessful to log it.");
        }

        AppendErrorMessage(logTextWriter, result.Error);
        IResultError? lastError = result.Error;
        logTextWriter.Indent++;
        while ((result = result?.Inner) is not null && result.Error is not null)
        {
            // ReSharper disable once PossibleUnintendedReferenceComparison
            if (lastError == result.Error)
            {
                continue;
            }
            lastError = result.Error;

            // logTextWriter.Indent++;
            logTextWriter.Write("---> ");
            AppendErrorMessage(logTextWriter, result.Error);
        }
    }

    private static void AppendErrorMessage(IndentedTextWriter indentedTextWriter, IResultError error)
    {
        var message = error is ExceptionError ex ? ex.Exception.ToString() : error.Message;
        indentedTextWriter.WriteLine($"{error.GetType().FullName}: {message}");
    }
}