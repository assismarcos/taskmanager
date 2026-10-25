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
    public static TaskResponse FromEntity(TaskItem task, DateOnly today) => throw new NotImplementedException();
}
