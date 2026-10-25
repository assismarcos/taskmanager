using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common;
using TaskManager.Domain.Tasks;

namespace TaskManager.Infrastructure.Persistence.Repositories;

public sealed class TaskRepository(AppDbContext context) : ITaskRepository
{
    public async Task<PagedResult<TaskItem>> ListAsync(
        Guid userId,
        TaskItemStatus? status,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var query = context.Tasks.AsNoTracking().Where(t => t.UserId == userId);
        if (status is not null)
        {
            query = query.Where(t => t.Status == status);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(t => t.DueDate)
            .ThenBy(t => t.CreatedAt)
            .ThenBy(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<TaskItem>(items, totalCount);
    }

    public Task<TaskItem?> GetByIdAsync(Guid userId, Guid taskId, CancellationToken ct) =>
        context.Tasks.FirstOrDefaultAsync(t => t.UserId == userId && t.Id == taskId, ct);

    public async Task AddAsync(TaskItem task, CancellationToken ct)
    {
        context.Tasks.Add(task);
        await context.SaveChangesAsync(ct);
    }

    public Task UpdateAsync(TaskItem task, CancellationToken ct) => context.SaveChangesAsync(ct);

    public async Task DeleteAsync(TaskItem task, CancellationToken ct)
    {
        context.Tasks.Remove(task);
        await context.SaveChangesAsync(ct);
    }
}
