using TaskManager.Domain.Tasks;

namespace TaskManager.Application.Tasks;

public sealed record UpdateTaskRequest(string Title, string? Description, DateOnly DueDate, TaskItemStatus Status);
