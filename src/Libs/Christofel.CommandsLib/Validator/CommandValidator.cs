//
//   CommandValidator.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;

namespace Christofel.CommandsLib.Validator
{
    /// <summary>
    /// Validator without a model that checks rules given to <see cref="MakeSure{T}"/>.
    /// </summary>
    public class CommandValidator
    {
        private readonly List<ValidationFailure> _results;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandValidator"/> class.
        /// </summary>
        public CommandValidator()
        {
            _results = new List<ValidationFailure>();
        }

        /// <summary>
        /// Validates given <paramref name="value"/> using <paramref name="builderAction"/> to build the rules for it.
        /// </summary>
        /// <param name="name">The name of the argument that will be shown in the failure result, if the parameter does not pass.</param>
        /// <param name="value">The value that will be checked.</param>
        /// <param name="builderAction">The action to configure validation of the argument.</param>
        /// <typeparam name="T">Type of the value to check.</typeparam>
        /// <returns>This validator for fluent API support.</returns>
        public CommandValidator MakeSure<T>(string name, T value, Action<IRuleBuilderInitial<T, T>> builderAction)
        {
            var errors = new ElementValidator<T>(builderAction).Validate(value).Errors;
            foreach (var error in errors)
            {
                error.PropertyName = name;
            }

            _results.AddRange(errors);
            return this;
        }

        /// <summary>
        /// Validates all the given rules.
        /// </summary>
        /// <returns>The result of the validation.</returns>
        public ValidationResult Validate() => new ValidationResult(_results);

        private class ElementValidator<T> : AbstractValidator<T>
        {
            public ElementValidator(Action<IRuleBuilderInitial<T, T>> builderAction)
            {
                builderAction(RuleFor(x => x));
            }
        }
    }
}