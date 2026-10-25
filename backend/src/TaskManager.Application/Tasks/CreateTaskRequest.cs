using TaskManager.Domain.Tasks;

namespace TaskManager.Application.Tasks;

public sealed record CreateTaskRequest(string Title, string? Description, DateOnly DueDate, TaskItemStatus? Status);
