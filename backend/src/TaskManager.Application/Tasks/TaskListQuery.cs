using TaskManager.Domain.Tasks;

namespace TaskManager.Application.Tasks;

public sealed record TaskListQuery(TaskItemStatus? Status, int Page = 1, int PageSize = 10);
