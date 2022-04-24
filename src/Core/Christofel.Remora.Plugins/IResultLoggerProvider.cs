//
//   IResultLoggerProvider.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.Remora;

/// <summary>
/// Provides result logging interface.
/// </summary>
public interface IResultLoggerProvider
{
    /// <summary>
    /// Log the given result.
    /// </summary>
    /// <param name="logger">The logger to log into.</param>
    /// <param name="result">The result to log.</param>
    /// <param name="message">The message to prepend.</param>
    public void Log(ILogger logger, IResult result, string message);
}