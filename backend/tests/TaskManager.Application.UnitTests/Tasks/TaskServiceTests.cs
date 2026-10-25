using Microsoft.Extensions.Time.Testing;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common;
using TaskManager.Application.Tasks;
using TaskManager.Application.UnitTests.Builders;
using TaskManager.Domain.Tasks;

namespace TaskManager.Application.UnitTests.Tasks;

public sealed class TaskServiceTests
{
    private static readonly DateOnly Today = new(2026, 6, 15);

    private readonly ITaskRepository _tasks = Substitute.For<ITaskRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly FakeTimeProvider _time = new(TaskItemBuilder.DefaultNow);
    private readonly Guid _userId = Guid.CreateVersion7();
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        _currentUser.UserId.Returns(_userId);
        _service = new TaskService(
            _tasks,
            _currentUser,
            new CreateTaskRequestValidator(_time),
            new UpdateTaskRequestValidator(),
            new TaskListQueryValidator(),
            _time);
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsTaskOwnedByCurrentUser()
    {
        // AC-3
        // Arrange
        TaskItem? saved = null;
        await _tasks.AddAsync(Arg.Do<TaskItem>(task => saved = task), Arg.Any<CancellationToken>());
        var request = new CreateTaskRequest(" Write report ", " notes ", Today, TaskItemStatus.InProgress);

        // Act
        var result = await _service.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Title.ShouldBe("Write report");
        result.Value.Description.ShouldBe("notes");
        result.Value.Status.ShouldBe(TaskItemStatus.InProgress);
        saved.ShouldNotBeNull();
        saved.UserId.ShouldBe(_userId);
        saved.Id.ShouldBe(result.Value.Id);
    }

    [Fact]
    public async Task GetById_WhenTaskBelongsToAnotherUser_ReturnsNotFound()
    {
        // AC-3
        // Arrange
        var taskId = Guid.CreateVersion7();
        _tasks.GetByIdAsync(_userId, taskId, Arg.Any<CancellationToken>()).Returns((TaskItem?)null);

        // Act
        var result = await _service.GetByIdAsync(taskId, CancellationToken.None);

        // Assert
        AssertNotFound(result);
    }

    [Fact]
    public async Task Update_WhenTaskBelongsToAnotherUser_ReturnsNotFound()
    {
        // AC-3
        // Arrange
        var taskId = Guid.CreateVersion7();
        _tasks.GetByIdAsync(_userId, taskId, Arg.Any<CancellationToken>()).Returns((TaskItem?)null);
        var request = new UpdateTaskRequest("New title", null, Today, TaskItemStatus.Todo);

        // Act
        var result = await _service.UpdateAsync(taskId, request, CancellationToken.None);

        // Assert
        AssertNotFound(result);
        await _tasks.DidNotReceive().UpdateAsync(Arg.Any<TaskItem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_WhenTaskBelongsToAnotherUser_ReturnsNotFound()
    {
        // AC-3
        // Arrange
        var taskId = Guid.CreateVersion7();
        _tasks.GetByIdAsync(_userId, taskId, Arg.Any<CancellationToken>()).Returns((TaskItem?)null);

        // Act
        var result = await _service.DeleteAsync(taskId, CancellationToken.None);

        // Assert
        AssertNotFound(result);
        await _tasks.DidNotReceive().DeleteAsync(Arg.Any<TaskItem>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(" ab ")]
    public async Task Create_WhenTitleIsEmpty_ReturnsValidationError(string title)
    {
        // AC-4
        // Arrange
        var request = new CreateTaskRequest(title, null, Today, null);

        // Act
        var result = await _service.CreateAsync(request, CancellationToken.None);

        // Assert
        AssertValidationFailure(result, "title");
        await _tasks.DidNotReceive().AddAsync(Arg.Any<TaskItem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_WhenDescriptionIsTooLong_ReturnsValidationError()
    {
        // AC-4
        // Arrange
        var request = new CreateTaskRequest("Write report", new string('x', 1001), Today, null);

        // Act
        var result = await _service.CreateAsync(request, CancellationToken.None);

        // Assert
        AssertValidationFailure(result, "description");
    }

    [Fact]
    public async Task Create_WhenDueDateIsInThePast_ReturnsValidationError()
    {
        // AC-5
        // Arrange
        var request = new CreateTaskRequest("Write report", null, Today.AddDays(-1), null);

        // Act
        var result = await _service.CreateAsync(request, CancellationToken.None);

        // Assert
        AssertValidationFailure(result, "dueDate");
    }

    [Fact]
    public async Task Update_WithPastDueDate_Succeeds()
    {
        // AC-5
        // Arrange
        var task = new TaskItemBuilder().OwnedBy(_userId).Build();
        _tasks.GetByIdAsync(_userId, task.Id, Arg.Any<CancellationToken>()).Returns(task);
        var request = new UpdateTaskRequest("New title", null, Today.AddDays(-3), TaskItemStatus.Todo);

        // Act
        var result = await _service.UpdateAsync(task.Id, request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.IsOverdue.ShouldBeTrue();
        task.Title.ShouldBe("New title");
        await _tasks.Received(1).UpdateAsync(task, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_FromDoneToInProgress_ReturnsInvalidStatusTransition()
    {
        // AC-6
        // Arrange
        var task = new TaskItemBuilder().OwnedBy(_userId).WithStatus(TaskItemStatus.Done).Build();
        _tasks.GetByIdAsync(_userId, task.Id, Arg.Any<CancellationToken>()).Returns(task);
        var request = new UpdateTaskRequest("New title", null, Today, TaskItemStatus.InProgress);

        // Act
        var result = await _service.UpdateAsync(task.Id, request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.Type.ShouldBe(ErrorType.Validation);
        result.Error.Code.ShouldBe("task.invalid_status_transition");
        result.Error.Message.ShouldBe("A Done task can only be reopened to Todo.");
        result.Error.Details.ShouldNotBeNull().ShouldContainKey("status");
        task.Status.ShouldBe(TaskItemStatus.Done);
        task.Title.ShouldBe("Write report");
        await _tasks.DidNotReceive().UpdateAsync(Arg.Any<TaskItem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_WithoutStatus_DefaultsToTodo()
    {
        // AC-6
        // Arrange
        var request = new CreateTaskRequest("Write report", null, Today, null);

        // Act
        var result = await _service.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Status.ShouldBe(TaskItemStatus.Todo);
    }

    [Fact]
    public async Task Delete_WhenTaskIsOwned_DeletesIt()
    {
        // AC-3
        // Arrange
        var task = new TaskItemBuilder().OwnedBy(_userId).Build();
        _tasks.GetByIdAsync(_userId, task.Id, Arg.Any<CancellationToken>()).Returns(task);

        // Act
        var result = await _service.DeleteAsync(task.Id, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _tasks.Received(1).DeleteAsync(task, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task List_PassesStatusAndPagingToRepository()
    {
        // AC-7
        // Arrange
        var task = new TaskItemBuilder().OwnedBy(_userId).Build();
        _tasks.ListAsync(_userId, TaskItemStatus.Done, 2, 5, Arg.Any<CancellationToken>())
            .Returns(new PagedResult<TaskItem>([task], 11));

        // Act
        var result = await _service.ListAsync(new TaskListQuery(TaskItemStatus.Done, 2, 5), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Page.ShouldBe(2);
        result.Value.PageSize.ShouldBe(5);
        result.Value.TotalCount.ShouldBe(11);
        result.Value.Items.Select(item => item.Id).ShouldBe([task.Id]);
    }

    [Fact]
    public async Task List_WhenQueryIsInvalid_ReturnsValidationErrorWithoutQueryingRepository()
    {
        // AC-7
        // Arrange
        var query = new TaskListQuery(null, 1, 51);

        // Act
        var result = await _service.ListAsync(query, CancellationToken.None);

        // Assert
        AssertValidationFailure(result, "pageSize");
        await _tasks.DidNotReceive().ListAsync(
            Arg.Any<Guid>(), Arg.Any<TaskItemStatus?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task List_MapsIsOverdueUsingTimeProvider()
    {
        // AC-8
        // Arrange
        var overdue = new TaskItemBuilder().OwnedBy(_userId).WithDueDate(Today.AddDays(-1)).Build();
        var dueToday = new TaskItemBuilder().OwnedBy(_userId).WithDueDate(Today).Build();
        var doneAndLate = new TaskItemBuilder()
            .OwnedBy(_userId).WithDueDate(Today.AddDays(-1)).WithStatus(TaskItemStatus.Done).Build();
        _tasks.ListAsync(_userId, null, 1, 10, Arg.Any<CancellationToken>())
            .Returns(new PagedResult<TaskItem>([overdue, dueToday, doneAndLate], 3));

        // Act
        var result = await _service.ListAsync(new TaskListQuery(null), CancellationToken.None);

        // Assert
        result.Value.Items.Select(item => item.IsOverdue).ShouldBe([true, false, false]);
    }

    private static void AssertNotFound(Result result)
    {
        result.IsSuccess.ShouldBeFalse();
        result.Error.Type.ShouldBe(ErrorType.NotFound);
        result.Error.Code.ShouldBe("task.not_found");
        result.Error.Message.ShouldBe("Task not found.");
    }

    private static void AssertValidationFailure(Result result, string field)
    {
        result.IsSuccess.ShouldBeFalse();
        result.Error.Type.ShouldBe(ErrorType.Validation);
        result.Error.Details.ShouldNotBeNull().ShouldContainKey(field);
    }
}
