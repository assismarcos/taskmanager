using FluentValidation;

namespace TaskManager.Application.Tasks;

public sealed class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator(TimeProvider timeProvider)
    {
        _ = timeProvider;
    }
}
