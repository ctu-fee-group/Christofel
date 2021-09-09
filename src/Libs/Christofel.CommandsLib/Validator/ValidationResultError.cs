//
//   ValidationResultError.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using FluentValidation.Results;
using Remora.Results;

namespace Christofel.CommandsLib.Validator
{
    public record ValidationResultError
    (
        IReadOnlyList<ValidationFailure> ValidationFailures
    ) : ResultError("Validation error occured.");
}