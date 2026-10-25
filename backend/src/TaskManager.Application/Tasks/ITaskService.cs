using TaskManager.Application.Common;

namespace TaskManager.Application.Tasks;

public interface ITaskService
{
    Task<Result<PagedResponse<TaskResponse>>> ListAsync(TaskListQuery query, CancellationToken ct);

    Task<Result<TaskResponse>> GetByIdAsync(Guid id, CancellationToken ct);

    Task<Result<TaskResponse>> CreateAsync(CreateTaskRequest request, CancellationToken ct);

    Task<Result<TaskResponse>> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken ct);

    Task<Result> DeleteAsync(Guid id, CancellationToken ct);
}
