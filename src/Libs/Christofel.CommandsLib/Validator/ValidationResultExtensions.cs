//
//   ValidationResultExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentValidation.Results;
using Remora.Results;

namespace Christofel.CommandsLib.Validator
{
    public static class ValidationResultExtensions
    {
        public static Result GetResult(this ValidationResult result) => result.IsValid
            ? Result.FromSuccess()
            : Result.FromError(new ValidationResultError(result.Errors));
    }
}