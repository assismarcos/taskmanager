using TaskManager.Application.Common;
using TaskManager.Domain.Tasks;

namespace TaskManager.Application.Abstractions;

public interface ITaskRepository
{
    Task<PagedResult<TaskItem>> ListAsync(
        Guid userId,
        TaskItemStatus? status,
        int page,
        int pageSize,
        CancellationToken ct);

    Task<TaskItem?> GetByIdAsync(Guid userId, Guid taskId, CancellationToken ct);

    Task AddAsync(TaskItem task, CancellationToken ct);

    Task UpdateAsync(TaskItem task, CancellationToken ct);

    Task DeleteAsync(TaskItem task, CancellationToken ct);
}
