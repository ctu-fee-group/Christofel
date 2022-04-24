//
//   ResultLoggerProvider.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.BaseLib.Extensions;
using Christofel.Remora;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.Helpers;

/// <summary>
/// The result logger.
/// </summary>
public class ResultLoggerProvider : IResultLoggerProvider
{
    /// <inheritdoc />
    public void Log(ILogger logger, IResult result, string message)
    {
        logger.LogResultError(result, message);
    }
}