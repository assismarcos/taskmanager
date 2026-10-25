using TaskManager.Domain.Tasks;

namespace TaskManager.Application.Tasks;

public sealed record TaskResponse(
    Guid Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    DateOnly DueDate,
    bool IsOverdue,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static TaskResponse FromEntity(TaskItem task, DateOnly today) => new(
        task.Id,
        task.Title,
        task.Description,
        task.Status,
        task.DueDate,
        task.IsOverdue(today),
        task.CreatedAt,
        task.UpdatedAt);
}
