using TaskManager.Domain.Common;
using TaskManager.Domain.Tasks;
using TaskManager.Domain.UnitTests.Builders;

namespace TaskManager.Domain.UnitTests.Tasks;

public class TaskItemTests
{
    private static readonly DateTimeOffset Now = TaskItemBuilder.DefaultNow;
    private static readonly DateOnly Today = DateOnly.FromDateTime(Now.UtcDateTime);

    [Fact]
    public void Create_WithValidData_SetsAllFieldsAndTimestamps()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var dueDate = Today.AddDays(3);

        // Act
        var task = TaskItem.Create(userId, "  Write report  ", "  Details  ", dueDate, TaskItemStatus.InProgress, Now);

        // Assert
        task.Id.ShouldNotBe(Guid.Empty);
        task.UserId.ShouldBe(userId);
        task.Title.ShouldBe("Write report");
        task.Description.ShouldBe("Details");
        task.Status.ShouldBe(TaskItemStatus.InProgress);
        task.DueDate.ShouldBe(dueDate);
        task.CreatedAt.ShouldBe(Now);
        task.UpdatedAt.ShouldBe(Now);
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
        var builder = new TaskItemBuilder().WithDueDate(Today.AddDays(-1));

        Should.Throw<DomainException>(() => builder.Build());
    }

    [Fact]
    public void Create_WhenDueDateIsToday_Succeeds()
    {
        // AC-5
        var task = new TaskItemBuilder().WithDueDate(Today).Build();

        task.DueDate.ShouldBe(Today);
    }
}
