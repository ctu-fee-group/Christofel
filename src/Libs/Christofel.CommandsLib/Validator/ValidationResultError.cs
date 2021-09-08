using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using Remora.Results;

namespace Christofel.CommandsLib.Validator
{
    public record ValidationResultError(
        IReadOnlyList<ValidationFailure> ValidationFailures
    ) : ResultError("Validation error occured.");
}