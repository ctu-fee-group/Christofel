//
//   UnexpectedContextError.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace Christofel.Helpers.Errors;

/// <summary>
/// The interaction context is unexpected. There is something
/// missing. The command cannot be served from this context.
/// </summary>
public record UnexpectedContextError(string Where, string ExpectedArgument)
: ResultError($"The interaction context was not expected in {Where}. The missing argument: {ExpectedArgument}");
