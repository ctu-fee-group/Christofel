using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;

namespace Christofel.CommandsLib.Validator
{
    public class CommandValidator
    {
        private List<ValidationFailure> _results;

        public CommandValidator()
        {
            _results = new List<ValidationFailure>();
        }

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

        public ValidationResult Validate()
        {
            return new ValidationResult(_results);
        }

        private class ElementValidator<T> : AbstractValidator<T>
        {
            public ElementValidator(Action<IRuleBuilderInitial<T, T>> builderAction)
            {
                builderAction(RuleFor(x => x));
            }
        }
    }
}