using TaskManager.Domain.Tasks;

namespace TaskManager.Application.UnitTests.Builders;

public sealed class TaskItemBuilder
{
    public static readonly DateTimeOffset DefaultNow = new(2026, 6, 15, 10, 0, 0, TimeSpan.Zero);

    private Guid _userId = Guid.CreateVersion7();
    private readonly string _title = "Write report";
    private readonly string? _description = "Quarterly numbers";
    private DateOnly _dueDate = DateOnly.FromDateTime(DefaultNow.UtcDateTime).AddDays(7);
    private TaskItemStatus _status = TaskItemStatus.Todo;

    public TaskItemBuilder OwnedBy(Guid userId)
    {
        _userId = userId;
        return this;
    }

    public TaskItemBuilder WithDueDate(DateOnly dueDate)
    {
        _dueDate = dueDate;
        return this;
    }

    public TaskItemBuilder WithStatus(TaskItemStatus status)
    {
        _status = status;
        return this;
    }

    public TaskItem Build()
    {
        // Create rejects past due dates, so an overdue task is created at the start of its due date.
        var dueDateStart = new DateTimeOffset(_dueDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var createdAt = dueDateStart < DefaultNow ? dueDateStart : DefaultNow;

        var task = TaskItem.Create(_userId, _title, _description, _dueDate, TaskItemStatus.Todo, createdAt);
        if (_status != TaskItemStatus.Todo)
        {
            task.ChangeStatus(_status, createdAt);
        }

        return task;
    }
}
