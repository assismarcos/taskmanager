using FluentValidation.Results;

namespace TaskManager.Application.Common;

public static class ValidationResultExtensions
{
    public static Error ToError(this ValidationResult result) => throw new NotImplementedException();
}
