using FluentValidation;

namespace TaskManager.Application.Tasks;

internal static class TaskFieldRules
{
    // Lengths are checked on the trimmed text, as TaskItem does.
    public static IRuleBuilderOptions<T, string> ValidTitle<T>(this IRuleBuilder<T, string> rule) => rule
        .Must(title => !string.IsNullOrWhiteSpace(title)).WithMessage("'{PropertyName}' must not be empty.")
        .Must(title => title.Trim().Length >= 3).WithMessage("'{PropertyName}' must be at least 3 characters.")
        .Must(title => title.Trim().Length <= 100).WithMessage("'{PropertyName}' must be at most 100 characters.");

    public static IRuleBuilderOptions<T, string?> ValidDescription<T>(this IRuleBuilder<T, string?> rule) => rule
        .Must(description => description is null || description.Trim().Length <= 1000)
        .WithMessage("'{PropertyName}' must be at most 1000 characters.");
}
