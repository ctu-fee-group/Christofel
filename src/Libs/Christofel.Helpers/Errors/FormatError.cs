//
//   FormatError.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace Christofel.Helpers.Errors;

/// <summary>
/// The format of the string or file is not recognized.
/// </summary>
/// <param name="Message">The actual error message.</param>
public record FormatError(string Message)
: ResultError(Message);
