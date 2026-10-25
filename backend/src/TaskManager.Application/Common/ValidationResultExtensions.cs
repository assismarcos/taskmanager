using FluentValidation.Results;

namespace TaskManager.Application.Common;

public static class ValidationResultExtensions
{
    public static Error ToError(this ValidationResult result)
    {
        var details = result.Errors
            .GroupBy(failure => ToCamelCase(failure.PropertyName))
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).ToArray());

        return new Error(ErrorType.Validation, "validation", "One or more validation errors occurred.", details);
    }

    private static string ToCamelCase(string name) =>
        name.Length == 0 ? name : char.ToLowerInvariant(name[0]) + name[1..];
}
