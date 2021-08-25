using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using Remora.Results;

namespace Christofel.CommandsLib.Validator
{
    public struct ValidationResultError : IResultError
    {
        public ValidationResultError(IReadOnlyList<ValidationFailure> validationFailures)
        {
            ValidationFailures = validationFailures;
        }

        public IReadOnlyList<ValidationFailure> ValidationFailures { get; }

        public string Message => string.Join("\n", ValidationFailures.Select(x => "  " + x.PropertyName + ": " + x.ErrorMessage));
    }
}