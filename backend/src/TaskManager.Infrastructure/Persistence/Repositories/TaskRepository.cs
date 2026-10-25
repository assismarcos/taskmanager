using TaskManager.Application.Abstractions;
using TaskManager.Application.Common;
using TaskManager.Domain.Tasks;

namespace TaskManager.Infrastructure.Persistence.Repositories;

public sealed class TaskRepository(AppDbContext context) : ITaskRepository
{
    public Task<PagedResult<TaskItem>> ListAsync(Guid userId, TaskItemStatus? status, int page, int pageSize, CancellationToken ct) =>
        throw new NotImplementedException();

    public Task<TaskItem?> GetByIdAsync(Guid userId, Guid taskId, CancellationToken ct) =>
        throw new NotImplementedException();

    public Task AddAsync(TaskItem task, CancellationToken ct) => context.Tasks.AddAsync(task, ct).AsTask();

    public Task UpdateAsync(TaskItem task, CancellationToken ct) => throw new NotImplementedException();

    public Task DeleteAsync(TaskItem task, CancellationToken ct) => throw new NotImplementedException();
}
