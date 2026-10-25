using FluentValidation;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common;

namespace TaskManager.Application.Tasks;

public sealed class TaskService(
    ITaskRepository tasks,
    ICurrentUser currentUser,
    IValidator<CreateTaskRequest> createValidator,
    IValidator<UpdateTaskRequest> updateValidator,
    IValidator<TaskListQuery> listValidator,
    TimeProvider timeProvider) : ITaskService
{
    private readonly object[] _skeleton = [tasks, currentUser, createValidator, updateValidator, listValidator, timeProvider];

    public Task<Result<PagedResponse<TaskResponse>>> ListAsync(TaskListQuery query, CancellationToken ct) =>
        throw new NotImplementedException();

    public Task<Result<TaskResponse>> GetByIdAsync(Guid id, CancellationToken ct) =>
        throw new NotImplementedException();

    public Task<Result<TaskResponse>> CreateAsync(CreateTaskRequest request, CancellationToken ct) =>
        throw new NotImplementedException();

    public Task<Result<TaskResponse>> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken ct) =>
        throw new NotImplementedException();

    public Task<Result> DeleteAsync(Guid id, CancellationToken ct) =>
        throw new NotImplementedException();
}
