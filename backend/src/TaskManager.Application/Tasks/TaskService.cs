using FluentValidation;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common;
using TaskManager.Domain.Tasks;

namespace TaskManager.Application.Tasks;

public sealed class TaskService(
    ITaskRepository tasks,
    ICurrentUser currentUser,
    IValidator<CreateTaskRequest> createValidator,
    IValidator<UpdateTaskRequest> updateValidator,
    IValidator<TaskListQuery> listValidator,
    TimeProvider timeProvider) : ITaskService
{
    private static readonly Error _notFound = new(ErrorType.NotFound, "task.not_found", "Task not found.");

    private static readonly Error _invalidTransition = new(
        ErrorType.Validation,
        "task.invalid_status_transition",
        "A Done task can only be reopened to Todo.",
        new Dictionary<string, string[]> { ["status"] = ["A Done task can only be reopened to Todo."] });

    public async Task<Result<PagedResponse<TaskResponse>>> ListAsync(TaskListQuery query, CancellationToken ct)
    {
        var validation = await listValidator.ValidateAsync(query, ct);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        var page = await tasks.ListAsync(currentUser.UserId, query.Status, query.Page, query.PageSize, ct);
        var today = Today();
        var items = page.Items.Select(task => TaskResponse.FromEntity(task, today)).ToList();

        return new PagedResponse<TaskResponse>(items, query.Page, query.PageSize, page.TotalCount);
    }

    public async Task<Result<TaskResponse>> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var task = await tasks.GetByIdAsync(currentUser.UserId, id, ct);
        if (task is null)
        {
            return _notFound;
        }

        return TaskResponse.FromEntity(task, Today());
    }

    public async Task<Result<TaskResponse>> CreateAsync(CreateTaskRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        var task = TaskItem.Create(
            currentUser.UserId,
            request.Title,
            request.Description,
            request.DueDate,
            request.Status ?? TaskItemStatus.Todo,
            timeProvider.GetUtcNow());
        await tasks.AddAsync(task, ct);

        return TaskResponse.FromEntity(task, Today());
    }

    public async Task<Result<TaskResponse>> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        var task = await tasks.GetByIdAsync(currentUser.UserId, id, ct);
        if (task is null)
        {
            return _notFound;
        }

        if (!task.CanChangeStatusTo(request.Status))
        {
            return _invalidTransition;
        }

        var now = timeProvider.GetUtcNow();
        task.Update(request.Title, request.Description, request.DueDate, now);
        task.ChangeStatus(request.Status, now);
        await tasks.UpdateAsync(task, ct);

        return TaskResponse.FromEntity(task, Today());
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct)
    {
        var task = await tasks.GetByIdAsync(currentUser.UserId, id, ct);
        if (task is null)
        {
            return _notFound;
        }

        await tasks.DeleteAsync(task, ct);

        return Result.Success();
    }

    private DateOnly Today() => DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
}
