using TaskManager.Domain.Tasks;

namespace TaskManager.Domain.UnitTests.Builders;

internal sealed class TaskItemBuilder
{
    public static readonly DateTimeOffset DefaultNow = new(2026, 6, 15, 10, 0, 0, TimeSpan.Zero);

    private string _title = "Write report";
    private string? _description = "Quarterly numbers";
    private DateOnly? _dueDate;
    private TaskItemStatus _status = TaskItemStatus.Todo;
    private DateTimeOffset _now = DefaultNow;

    public TaskItemBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public TaskItemBuilder WithDescription(string? description)
    {
        _description = description;
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

    public TaskItemBuilder WithNow(DateTimeOffset now)
    {
        _now = now;
        return this;
    }

    public TaskItem Build()
    {
        var dueDate = _dueDate ?? DateOnly.FromDateTime(_now.UtcDateTime).AddDays(7);
        return TaskItem.Create(Guid.CreateVersion7(), _title, _description, dueDate, _status, _now);
    }
}
