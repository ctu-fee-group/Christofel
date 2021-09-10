//
//   ValidationResultExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentValidation.Results;
using Remora.Results;

namespace Christofel.CommandsLib.Validator
{
    /// <summary>
    /// Class containing extensions for <see cref="ValidationResult"/>.
    /// </summary>
    public static class ValidationResultExtensions
    {
        /// <summary>
        /// Converts the <see cref="result"/> into <see cref="Result"/> so it can be returned from command handler.
        /// </summary>
        /// <param name="result">The result of the validation.</param>
        /// <returns>The result containing validation information. <see cref="ValidationResultError"/> in case of an error.</returns>
        public static Result GetResult(this ValidationResult result) => result.IsValid
            ? Result.FromSuccess()
            : Result.FromError(new ValidationResultError(result.Errors));
    }
}