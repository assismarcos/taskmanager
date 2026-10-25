using FluentValidation;

namespace TaskManager.Application.Tasks;

public sealed class TaskListQueryValidator : AbstractValidator<TaskListQuery>
{
    // The page cap keeps (page - 1) * pageSize from overflowing an int.
    public TaskListQueryValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(query => query.Status).IsInEnum().When(query => query.Status.HasValue);
        RuleFor(query => query.Page).InclusiveBetween(1, 10000);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 50);
    }
}
