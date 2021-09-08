using FluentValidation.Results;
using Remora.Results;

namespace Christofel.CommandsLib.Validator
{
    public static class ValidationResultExtensions
    {
        public static Result GetResult(this ValidationResult result)
        {
            return result.IsValid
                ? Result.FromSuccess()
                : Result.FromError(new ValidationResultError(result.Errors));
        }
    }
}