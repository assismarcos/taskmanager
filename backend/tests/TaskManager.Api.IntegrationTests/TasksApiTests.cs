using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TaskManager.Api.IntegrationTests.Fixtures;
using TaskManager.Application.Common;
using TaskManager.Application.Tasks;
using TaskManager.Domain.Tasks;

namespace TaskManager.Api.IntegrationTests;

public sealed class TasksApiTests(PostgresFixture database) : ApiTestBase(database)
{
    private static CancellationToken _ct => TestContext.Current.CancellationToken;

    public static TheoryData<string, string> ProtectedEndpoints => new()
    {
        { "GET", "/api/tasks" },
        { "GET", "/api/tasks/0193a5b3-1a2b-7c3d-8e4f-5a6b7c8d9e0f" },
        { "POST", "/api/tasks" },
        { "PUT", "/api/tasks/0193a5b3-1a2b-7c3d-8e4f-5a6b7c8d9e0f" },
        { "DELETE", "/api/tasks/0193a5b3-1a2b-7c3d-8e4f-5a6b7c8d9e0f" },
    };

    [Theory]
    [MemberData(nameof(ProtectedEndpoints))]
    public async Task TaskEndpoints_WithoutToken_Return401(string method, string url)
    {
        // Arrange
        var request = new HttpRequestMessage(new HttpMethod(method), url);

        // Act
        var response = await Client.SendAsync(request, _ct);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized); // AC-9
    }

    [Fact]
    public async Task CreateThenGet_ReturnsSameTask()
    {
        // Arrange
        var client = await AuthenticateAsync();

        // Act
        var created = await client.PostAsJsonAsync("/api/tasks", TaskBody("Prepare the demo", Today), _ct);
        var task = await created.Content.ReadFromJsonAsync<TaskResponse>(Json, _ct);
        var fetched = await client.GetFromJsonAsync<TaskResponse>(created.Headers.Location, Json, _ct);

        // Assert
        created.StatusCode.ShouldBe(HttpStatusCode.Created); // AC-3
        created.Headers.Location!.AbsolutePath.ShouldBe($"/api/tasks/{task!.Id}");
        fetched!.Id.ShouldBe(task.Id);
        fetched.Title.ShouldBe(task.Title);
        fetched.DueDate.ShouldBe(task.DueDate);
        task.Status.ShouldBe(TaskItemStatus.Todo);
    }

    [Fact]
    public async Task GetById_TaskOfAnotherUser_Returns404()
    {
        // Arrange
        var owner = await AuthenticateAsync("owner@mail.com");
        var other = await AuthenticateAsync("other@mail.com");
        var id = await CreateTaskAsync(owner);

        // Act
        var response = await other.GetAsync($"/api/tasks/{id}", _ct);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound); // AC-3
    }

    [Fact]
    public async Task Update_TaskOfAnotherUser_Returns404()
    {
        // Arrange
        var owner = await AuthenticateAsync("owner@mail.com");
        var other = await AuthenticateAsync("other@mail.com");
        var id = await CreateTaskAsync(owner);

        // Act
        var response = await other.PutAsJsonAsync($"/api/tasks/{id}", TaskBody("Hijacked title", Today, "Done"), _ct);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound); // AC-3
    }

    [Fact]
    public async Task Delete_TaskOfAnotherUser_Returns404()
    {
        // Arrange
        var owner = await AuthenticateAsync("owner@mail.com");
        var other = await AuthenticateAsync("other@mail.com");
        var id = await CreateTaskAsync(owner);

        // Act
        var response = await other.DeleteAsync($"/api/tasks/{id}", _ct);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound); // AC-3
        (await owner.GetAsync($"/api/tasks/{id}", _ct)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_WithPastDueDate_Returns400WithFieldError()
    {
        // Arrange
        var client = await AuthenticateAsync();

        // Act
        var response = await client.PostAsJsonAsync("/api/tasks", TaskBody("Prepare the demo", Today.AddDays(-1)), _ct);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest); // AC-5
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(_ct);
        problem.GetProperty("errors").TryGetProperty("dueDate", out _).ShouldBeTrue();
    }

    [Fact]
    public async Task Update_WithMissingDueDate_Returns400()
    {
        // Arrange
        var client = await AuthenticateAsync();
        var id = await CreateTaskAsync(client);

        // Act
        var response = await client.PutAsJsonAsync($"/api/tasks/{id}", new { title = "Prepare the demo", status = "Todo" }, _ct);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WithMissingStatus_Returns400()
    {
        // Arrange
        var client = await AuthenticateAsync();
        var id = await CreateTaskAsync(client);

        // Act
        var response = await client.PutAsJsonAsync($"/api/tasks/{id}", new { title = "Prepare the demo", dueDate = Today }, _ct);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithoutDescriptionAndStatus_Returns201WithTodo()
    {
        // Arrange
        var client = await AuthenticateAsync();

        // Act
        var response = await client.PostAsJsonAsync("/api/tasks", new { title = "Prepare the demo", dueDate = Today }, _ct);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var task = await response.Content.ReadFromJsonAsync<TaskResponse>(Json, _ct);
        task!.Status.ShouldBe(TaskItemStatus.Todo);
        task.Description.ShouldBeNull();
    }

    [Fact]
    public async Task List_FilteredByStatus_ReturnsOnlyMatchingTasks()
    {
        // Arrange
        var client = await AuthenticateAsync();
        await CreateTaskAsync(client, "First todo task");
        await CreateTaskAsync(client, "Second done task", "Done");

        // Act
        var page = await client.GetFromJsonAsync<PagedResponse<TaskResponse>>("/api/tasks?status=Done", Json, _ct);

        // Assert
        page!.TotalCount.ShouldBe(1); // AC-7
        page.Items.Single().Title.ShouldBe("Second done task");
    }

    [Fact]
    public async Task List_AfterDueDatePasses_MarksTaskAsOverdue()
    {
        // Arrange
        var client = await AuthenticateAsync();
        await CreateTaskAsync(client);

        // Act
        var before = await client.GetFromJsonAsync<PagedResponse<TaskResponse>>("/api/tasks", Json, _ct);
        Time.Advance(TimeSpan.FromDays(2));
        var after = await client.GetFromJsonAsync<PagedResponse<TaskResponse>>("/api/tasks", Json, _ct);

        // Assert
        before!.Items.Single().IsOverdue.ShouldBeFalse();
        after!.Items.Single().IsOverdue.ShouldBeTrue(); // AC-8
    }

    private async Task<Guid> CreateTaskAsync(HttpClient client, string title = "Prepare the demo", string status = "Todo")
    {
        var response = await client.PostAsJsonAsync("/api/tasks", TaskBody(title, Today, status), _ct);
        response.EnsureSuccessStatusCode();
        var task = await response.Content.ReadFromJsonAsync<TaskResponse>(Json, _ct);
        return task!.Id;
    }
}
