using TaskManager.Domain.Common;

namespace TaskManager.Domain.Tasks;

public sealed class TaskItem
{
    private const int TitleMinLength = 3;
    private const int TitleMaxLength = 100;
    private const int DescriptionMaxLength = 1000;

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
        DateTimeOffset now)
    {
        var cleanTitle = NormalizeTitle(title);
        var cleanDescription = NormalizeDescription(description);

        if (dueDate < DateOnly.FromDateTime(now.UtcDateTime))
        {
            throw new DomainException("Due date cannot be in the past.");
        }

        return new TaskItem(Guid.CreateVersion7(), userId, cleanTitle, cleanDescription, status, dueDate, now, now);
    }

    public void Update(string title, string? description, DateOnly dueDate, DateTimeOffset now)
    {
        Title = NormalizeTitle(title);
        Description = NormalizeDescription(description);
        DueDate = dueDate; // AC-5: the past-date rule applies on creation only
        UpdatedAt = now;
    }

    public bool CanChangeStatusTo(TaskItemStatus newStatus) =>
        newStatus == Status || !(Status == TaskItemStatus.Done && newStatus == TaskItemStatus.InProgress);

    public void ChangeStatus(TaskItemStatus newStatus, DateTimeOffset now)
    {
        if (!CanChangeStatusTo(newStatus))
        {
            throw new DomainException($"A {Status} task cannot move to {newStatus}.");
        }

        if (newStatus == Status)
        {
            return;
        }

        Status = newStatus;
        UpdatedAt = now;
    }

    public bool IsOverdue(DateOnly today) => Status != TaskItemStatus.Done && DueDate < today;

    private static string NormalizeTitle(string title)
    {
        var trimmed = title?.Trim() ?? string.Empty;
        if (trimmed.Length is < TitleMinLength or > TitleMaxLength)
        {
            throw new DomainException($"Title must be between {TitleMinLength} and {TitleMaxLength} characters.");
        }

        return trimmed;
    }

    private static string? NormalizeDescription(string? description)
    {
        var trimmed = description?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        if (trimmed.Length > DescriptionMaxLength)
        {
            throw new DomainException($"Description must be at most {DescriptionMaxLength} characters.");
        }

        return trimmed;
    }
}
