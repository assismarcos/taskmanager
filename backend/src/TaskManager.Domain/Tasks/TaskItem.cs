namespace TaskManager.Domain.Tasks;

public sealed class TaskItem
{
    // Also used by EF Core to materialize rows.
    private TaskItem(
        Guid id,
        Guid userId,
        string title,
        string? description,
        TaskItemStatus status,
        DateOnly dueDate,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        Id = id;
        UserId = userId;
        Title = title;
        Description = description;
        Status = status;
        DueDate = dueDate;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Title { get; private set; }

    public string? Description { get; private set; }

    public TaskItemStatus Status { get; private set; }

    public DateOnly DueDate { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static TaskItem Create(
        Guid userId,
        string title,
        string? description,
        DateOnly dueDate,
        TaskItemStatus status,
        DateTimeOffset now) =>
        throw new NotImplementedException();

    public void Update(string title, string? description, DateOnly dueDate, DateTimeOffset now) =>
        throw new NotImplementedException();

    public bool CanChangeStatusTo(TaskItemStatus newStatus) => throw new NotImplementedException();

    public void ChangeStatus(TaskItemStatus newStatus, DateTimeOffset now) => throw new NotImplementedException();

    public bool IsOverdue(DateOnly today) => throw new NotImplementedException();
}
