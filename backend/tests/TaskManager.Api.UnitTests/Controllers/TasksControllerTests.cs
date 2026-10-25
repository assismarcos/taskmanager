using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Controllers;
using TaskManager.Application.Common;
using TaskManager.Application.Tasks;
using TaskManager.Domain.Tasks;

namespace TaskManager.Api.UnitTests.Controllers;

public sealed class TasksControllerTests
{
    private static readonly TaskResponse _task = new(
        Guid.NewGuid(),
        "Prepare the demo",
        null,
        TaskItemStatus.Todo,
        new DateOnly(2026, 10, 30),
        false,
        DateTimeOffset.UnixEpoch,
        DateTimeOffset.UnixEpoch);

    private readonly ITaskService _taskService = Substitute.For<ITaskService>();
    private readonly TasksController _controller;

    public TasksControllerTests() => _controller = new TasksController(_taskService);

    [Fact]
    public async Task Create_WhenSuccessful_Returns201WithLocation()
    {
        // Arrange
        var request = new CreateTaskRequest("Prepare the demo", null, new DateOnly(2026, 10, 30), null);
        _taskService.CreateAsync(request, Arg.Any<CancellationToken>()).Returns(_task);

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        var created = result.ShouldBeOfType<CreatedAtActionResult>();
        created.ActionName.ShouldBe(nameof(TasksController.GetById));
        created.RouteValues!["id"].ShouldBe(_task.Id);
        created.Value.ShouldBe(_task);
    }

    [Fact]
    public async Task Create_WhenValidationFails_Returns400ValidationProblem()
    {
        // Arrange
        var request = new CreateTaskRequest("x", null, new DateOnly(2026, 10, 30), null);
        var details = new Dictionary<string, string[]> { ["title"] = ["Title is too short."] };
        _taskService.CreateAsync(request, Arg.Any<CancellationToken>())
            .Returns(new Error(ErrorType.Validation, "validation", "Validation failed.", details));

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        var objectResult = result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(400);
        objectResult.Value.ShouldBeOfType<ValidationProblemDetails>().Errors.ShouldContainKey("title");
    }

    [Fact]
    public async Task GetById_WhenNotFound_Returns404()
    {
        // Arrange
        _taskService.GetByIdAsync(_task.Id, Arg.Any<CancellationToken>())
            .Returns(new Error(ErrorType.NotFound, "task.not_found", "Task not found."));

        // Act
        var result = await _controller.GetById(_task.Id, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<ObjectResult>().StatusCode.ShouldBe(404);
    }

    [Fact]
    public async Task Update_WhenSuccessful_Returns200()
    {
        // Arrange
        var request = new UpdateTaskRequest("Prepare the demo", null, new DateOnly(2026, 10, 30), TaskItemStatus.Done);
        _taskService.UpdateAsync(_task.Id, request, Arg.Any<CancellationToken>()).Returns(_task);

        // Act
        var result = await _controller.Update(_task.Id, request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(_task);
    }

    [Fact]
    public async Task Delete_WhenSuccessful_Returns204()
    {
        // Arrange
        _taskService.DeleteAsync(_task.Id, Arg.Any<CancellationToken>()).Returns(Result.Success());

        // Act
        var result = await _controller.Delete(_task.Id, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public async Task List_ReturnsPagedResponse()
    {
        // Arrange
        var query = new TaskListQuery(TaskItemStatus.Todo, 1, 10);
        var page = new PagedResponse<TaskResponse>([_task], 1, 10, 1);
        _taskService.ListAsync(query, Arg.Any<CancellationToken>()).Returns(page);

        // Act
        var result = await _controller.List(query, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(page);
    }
}
