using FluentValidation;

namespace TaskManager.Application.Tasks;

public sealed class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(request => request.Title).ValidTitle();
        RuleFor(request => request.Description).ValidDescription();
        RuleFor(request => request.Status).IsInEnum();
    }
}
