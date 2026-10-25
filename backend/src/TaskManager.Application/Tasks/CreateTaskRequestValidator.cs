using FluentValidation;

namespace TaskManager.Application.Tasks;

public sealed class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator(TimeProvider timeProvider)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(request => request.Title).ValidTitle();
        RuleFor(request => request.Description).ValidDescription();
        RuleFor(request => request.DueDate)
            .Must(dueDate => dueDate >= DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime))
            .WithMessage("'{PropertyName}' must be today or later.");
        RuleFor(request => request.Status).IsInEnum().When(request => request.Status.HasValue);
    }
}
