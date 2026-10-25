using TaskManager.Domain.Common;
using TaskManager.Domain.Tasks;
using TaskManager.Domain.UnitTests.Builders;

namespace TaskManager.Domain.UnitTests.Tasks;

public class TaskItemTests
{
    private static readonly DateTimeOffset _now = TaskItemBuilder.DefaultNow;
    private static readonly DateOnly _today = DateOnly.FromDateTime(_now.UtcDateTime);

    [Fact]
    public void Create_WithValidData_SetsAllFieldsAndTimestamps()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var dueDate = _today.AddDays(3);

        // Act
        var task = TaskItem.Create(userId, "  Write report  ", "  Details  ", dueDate, TaskItemStatus.InProgress, _now);

        // Assert
        task.Id.ShouldNotBe(Guid.Empty);
        task.UserId.ShouldBe(userId);
        task.Title.ShouldBe("Write report");
        task.Description.ShouldBe("Details");
        task.Status.ShouldBe(TaskItemStatus.InProgress);
        task.DueDate.ShouldBe(dueDate);
        task.CreatedAt.ShouldBe(_now);
        task.UpdatedAt.ShouldBe(_now);
    }

    [Fact]
    public void Create_WhenTitleIsShorterThan3Chars_ThrowsDomainException()
    {
        // AC-4
        var builder = new TaskItemBuilder().WithTitle(" ab ");

        Should.Throw<DomainException>(() => builder.Build());
    }

    [Fact]
    public void Create_WhenTitleIsLongerThan100Chars_ThrowsDomainException()
    {
        // AC-4
        var builder = new TaskItemBuilder().WithTitle(new string('a', 101));

        Should.Throw<DomainException>(() => builder.Build());
    }

    [Fact]
    public void Create_WhenDescriptionIsLongerThan1000Chars_ThrowsDomainException()
    {
        // AC-4
        var builder = new TaskItemBuilder().WithDescription(new string('a', 1001));

        Should.Throw<DomainException>(() => builder.Build());
    }

    [Fact]
    public void Create_WithWhitespaceDescription_StoresNull()
    {
        // AC-4
        var task = new TaskItemBuilder().WithDescription("   ").Build();

        task.Description.ShouldBeNull();
    }

    [Fact]
    public void Create_WhenDueDateIsInThePast_ThrowsDomainException()
    {
        // AC-5
        var builder = new TaskItemBuilder().WithDueDate(_today.AddDays(-1));

        Should.Throw<DomainException>(() => builder.Build());
    }

    [Fact]
    public void Create_WhenDueDateIsToday_Succeeds()
    {
        // AC-5
        var task = new TaskItemBuilder().WithDueDate(_today).Build();

        task.DueDate.ShouldBe(_today);
    }

    [Fact]
    public void Update_WhenDueDateIsInThePast_Succeeds()
    {
        // AC-5
        var task = new TaskItemBuilder().Build();
        var pastDate = _today.AddDays(-5);
        var later = _now.AddDays(1);

        task.Update("New title", "New description", pastDate, later);

        task.Title.ShouldBe("New title");
        task.Description.ShouldBe("New description");
        task.DueDate.ShouldBe(pastDate);
        task.UpdatedAt.ShouldBe(later);
    }

    [Fact]
    public void ChangeStatus_FromTodoToDone_Succeeds()
    {
        // AC-6
        var task = new TaskItemBuilder().WithStatus(TaskItemStatus.Todo).Build();

        task.ChangeStatus(TaskItemStatus.Done, _now);

        task.Status.ShouldBe(TaskItemStatus.Done);
    }

    [Fact]
    public void ChangeStatus_FromDoneToTodo_Succeeds()
    {
        // AC-6
        var task = new TaskItemBuilder().WithStatus(TaskItemStatus.Done).Build();

        task.ChangeStatus(TaskItemStatus.Todo, _now);

        task.Status.ShouldBe(TaskItemStatus.Todo);
    }

    [Fact]
    public void ChangeStatus_FromDoneToInProgress_ThrowsDomainException()
    {
        // AC-6
        var task = new TaskItemBuilder().WithStatus(TaskItemStatus.Done).Build();

        Should.Throw<DomainException>(() => task.ChangeStatus(TaskItemStatus.InProgress, _now));
    }

    [Fact]
    public void CanChangeStatusTo_FromDoneToInProgress_ReturnsFalse()
    {
        // AC-6
        var task = new TaskItemBuilder().WithStatus(TaskItemStatus.Done).Build();

        task.CanChangeStatusTo(TaskItemStatus.InProgress).ShouldBeFalse();
    }

    [Fact]
    public void ChangeStatus_UpdatesUpdatedAt()
    {
        var task = new TaskItemBuilder().Build();
        var later = _now.AddHours(2);

        task.ChangeStatus(TaskItemStatus.InProgress, later);

        task.UpdatedAt.ShouldBe(later);
        task.CreatedAt.ShouldBe(_now);
    }

    [Fact]
    public void IsOverdue_WhenNotDoneAndDueDateHasPassed_ReturnsTrue()
    {
        // AC-8
        var task = new TaskItemBuilder().WithDueDate(_today).Build();

        task.IsOverdue(_today.AddDays(1)).ShouldBeTrue();
    }

    [Fact]
    public void IsOverdue_WhenDone_ReturnsFalse()
    {
        // AC-8
        var task = new TaskItemBuilder().WithDueDate(_today).WithStatus(TaskItemStatus.Done).Build();

        task.IsOverdue(_today.AddDays(1)).ShouldBeFalse();
    }

    [Fact]
    public void IsOverdue_WhenDueDateIsToday_ReturnsFalse()
    {
        // AC-8
        var task = new TaskItemBuilder().WithDueDate(_today).Build();

        task.IsOverdue(_today).ShouldBeFalse();
    }
}
