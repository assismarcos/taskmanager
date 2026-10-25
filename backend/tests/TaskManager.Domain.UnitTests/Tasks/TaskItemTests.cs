using TaskManager.Domain.Common;
using TaskManager.Domain.Tasks;
using TaskManager.Domain.UnitTests.Builders;

namespace TaskManager.Domain.UnitTests.Tasks;

public sealed class TaskItemTests
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
        // Arrange
        var builder = new TaskItemBuilder().WithTitle(" ab ");

        // Act
        var act = () => builder.Build();

        // Assert
        act.ShouldThrow<DomainException>();
    }

    [Fact]
    public void Create_WhenTitleIsLongerThan100Chars_ThrowsDomainException()
    {
        // AC-4
        // Arrange
        var builder = new TaskItemBuilder().WithTitle(new string('a', 101));

        // Act
        var act = () => builder.Build();

        // Assert
        act.ShouldThrow<DomainException>();
    }

    [Fact]
    public void Create_WhenDescriptionIsLongerThan1000Chars_ThrowsDomainException()
    {
        // AC-4
        // Arrange
        var builder = new TaskItemBuilder().WithDescription(new string('a', 1001));

        // Act
        var act = () => builder.Build();

        // Assert
        act.ShouldThrow<DomainException>();
    }

    [Fact]
    public void Create_WithWhitespaceDescription_StoresNull()
    {
        // AC-4
        // Arrange
        var builder = new TaskItemBuilder().WithDescription("   ");

        // Act
        var task = builder.Build();

        // Assert
        task.Description.ShouldBeNull();
    }

    [Fact]
    public void Create_WhenDueDateIsInThePast_ThrowsDomainException()
    {
        // AC-5
        // Arrange
        var builder = new TaskItemBuilder().WithDueDate(_today.AddDays(-1));

        // Act
        var act = () => builder.Build();

        // Assert
        act.ShouldThrow<DomainException>();
    }

    [Fact]
    public void Create_WhenDueDateIsToday_Succeeds()
    {
        // AC-5
        // Arrange
        var builder = new TaskItemBuilder().WithDueDate(_today);

        // Act
        var task = builder.Build();

        // Assert
        task.DueDate.ShouldBe(_today);
    }

    [Fact]
    public void Update_WhenDueDateIsInThePast_Succeeds()
    {
        // AC-5
        // Arrange
        var task = new TaskItemBuilder().Build();
        var pastDate = _today.AddDays(-5);
        var later = _now.AddDays(1);

        // Act
        task.Update("New title", "New description", pastDate, later);

        // Assert
        task.Title.ShouldBe("New title");
        task.Description.ShouldBe("New description");
        task.DueDate.ShouldBe(pastDate);
        task.UpdatedAt.ShouldBe(later);
    }

    [Fact]
    public void ChangeStatus_FromTodoToDone_Succeeds()
    {
        // AC-6
        // Arrange
        var task = new TaskItemBuilder().WithStatus(TaskItemStatus.Todo).Build();

        // Act
        task.ChangeStatus(TaskItemStatus.Done, _now);

        // Assert
        task.Status.ShouldBe(TaskItemStatus.Done);
    }

    [Fact]
    public void ChangeStatus_FromDoneToTodo_Succeeds()
    {
        // AC-6
        // Arrange
        var task = new TaskItemBuilder().WithStatus(TaskItemStatus.Done).Build();

        // Act
        task.ChangeStatus(TaskItemStatus.Todo, _now);

        // Assert
        task.Status.ShouldBe(TaskItemStatus.Todo);
    }

    [Fact]
    public void ChangeStatus_FromDoneToInProgress_ThrowsDomainException()
    {
        // AC-6
        // Arrange
        var task = new TaskItemBuilder().WithStatus(TaskItemStatus.Done).Build();

        // Act
        var act = () => task.ChangeStatus(TaskItemStatus.InProgress, _now.AddHours(1));

        // Assert
        act.ShouldThrow<DomainException>();
        task.Status.ShouldBe(TaskItemStatus.Done);
        task.UpdatedAt.ShouldBe(_now);
    }

    [Theory]
    [InlineData(TaskItemStatus.Todo, TaskItemStatus.Todo, true)]
    [InlineData(TaskItemStatus.Todo, TaskItemStatus.InProgress, true)]
    [InlineData(TaskItemStatus.Todo, TaskItemStatus.Done, true)]
    [InlineData(TaskItemStatus.InProgress, TaskItemStatus.Todo, true)]
    [InlineData(TaskItemStatus.InProgress, TaskItemStatus.InProgress, true)]
    [InlineData(TaskItemStatus.InProgress, TaskItemStatus.Done, true)]
    [InlineData(TaskItemStatus.Done, TaskItemStatus.Todo, true)]
    [InlineData(TaskItemStatus.Done, TaskItemStatus.InProgress, false)]
    [InlineData(TaskItemStatus.Done, TaskItemStatus.Done, true)]
    public void CanChangeStatusTo_ReturnsExpectedResult(TaskItemStatus from, TaskItemStatus to, bool expected)
    {
        // AC-6
        // Arrange
        var task = new TaskItemBuilder().WithStatus(from).Build();

        // Act
        var result = task.CanChangeStatusTo(to);

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void CanChangeStatusTo_ToSameStatus_ReturnsTrue()
    {
        // AC-6
        // Arrange
        var task = new TaskItemBuilder().WithStatus(TaskItemStatus.Done).Build();

        // Act
        var result = task.CanChangeStatusTo(TaskItemStatus.Done);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void ChangeStatus_ToSameStatus_LeavesUpdatedAtUnchanged()
    {
        // AC-6
        // Arrange
        var task = new TaskItemBuilder().WithStatus(TaskItemStatus.InProgress).Build();

        // Act
        task.ChangeStatus(TaskItemStatus.InProgress, _now.AddHours(3));

        // Assert
        task.Status.ShouldBe(TaskItemStatus.InProgress);
        task.UpdatedAt.ShouldBe(_now);
    }

    [Fact]
    public void Update_WhenTitleIsShorterThan3Chars_ThrowsDomainException()
    {
        // AC-4
        // Arrange
        var task = new TaskItemBuilder().Build();

        // Act
        var act = () => task.Update(" ab ", null, _today, _now);

        // Assert
        act.ShouldThrow<DomainException>();
    }

    [Fact]
    public void Update_WithWhitespaceDescription_StoresNull()
    {
        // AC-4
        // Arrange
        var task = new TaskItemBuilder().Build();

        // Act
        task.Update("New title", "   ", _today, _now);

        // Assert
        task.Description.ShouldBeNull();
    }

    [Theory]
    [InlineData(3, 0)]
    [InlineData(100, 0)]
    [InlineData(5, 1000)]
    public void Create_WithBoundaryLengths_Succeeds(int titleLength, int descriptionLength)
    {
        // AC-4
        // Arrange
        var builder = new TaskItemBuilder()
            .WithTitle(new string('a', titleLength))
            .WithDescription(new string('b', descriptionLength));

        // Act
        var task = builder.Build();

        // Assert
        task.Title.Length.ShouldBe(titleLength);
        (task.Description?.Length ?? 0).ShouldBe(descriptionLength);
    }

    [Fact]
    public void Create_WhenDueDateIsBeforeTheUtcDateOfNow_ThrowsDomainException()
    {
        // AC-5
        // Arrange
        var localNow = new DateTimeOffset(2026, 6, 15, 22, 0, 0, TimeSpan.FromHours(-5));
        var builder = new TaskItemBuilder().WithNow(localNow).WithDueDate(new DateOnly(2026, 6, 15));

        // Act
        var act = () => builder.Build();

        // Assert
        act.ShouldThrow<DomainException>();
    }

    [Fact]
    public void CanChangeStatusTo_FromDoneToInProgress_ReturnsFalse()
    {
        // AC-6
        // Arrange
        var task = new TaskItemBuilder().WithStatus(TaskItemStatus.Done).Build();

        // Act
        var result = task.CanChangeStatusTo(TaskItemStatus.InProgress);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ChangeStatus_UpdatesUpdatedAt()
    {
        // Arrange
        var task = new TaskItemBuilder().Build();
        var later = _now.AddHours(2);

        // Act
        task.ChangeStatus(TaskItemStatus.InProgress, later);

        // Assert
        task.UpdatedAt.ShouldBe(later);
        task.CreatedAt.ShouldBe(_now);
    }

    [Fact]
    public void IsOverdue_WhenNotDoneAndDueDateHasPassed_ReturnsTrue()
    {
        // AC-8
        // Arrange
        var task = new TaskItemBuilder().WithDueDate(_today).Build();

        // Act
        var result = task.IsOverdue(_today.AddDays(1));

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsOverdue_WhenDone_ReturnsFalse()
    {
        // AC-8
        // Arrange
        var task = new TaskItemBuilder().WithDueDate(_today).WithStatus(TaskItemStatus.Done).Build();

        // Act
        var result = task.IsOverdue(_today.AddDays(1));

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsOverdue_WhenDueDateIsToday_ReturnsFalse()
    {
        // AC-8
        // Arrange
        var task = new TaskItemBuilder().WithDueDate(_today).Build();

        // Act
        var result = task.IsOverdue(_today);

        // Assert
        result.ShouldBeFalse();
    }
}
