using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Tasks;
using TaskManager.Domain.Users;
using TaskManager.Infrastructure.IntegrationTests.Fixtures;
using TaskManager.Infrastructure.Persistence.Repositories;

namespace TaskManager.Infrastructure.IntegrationTests.Repositories;

[Collection("Postgres")]
[Trait("Category", "Integration")]
public sealed class TaskRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    private static readonly DateTimeOffset _now = new(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly _due = new(2026, 2, 1);

    public async ValueTask InitializeAsync() => await fixture.ResetAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private async Task<User> SeedUserAsync(string email)
    {
        await using var context = fixture.CreateContext();
        var user = User.Create("Test", email, "hash", _now);
        context.Users.Add(user);
        await context.SaveChangesAsync(Ct);
        return user;
    }

    private async Task SeedTasksAsync(params TaskItem[] tasks)
    {
        await using var context = fixture.CreateContext();
        context.Tasks.AddRange(tasks);
        await context.SaveChangesAsync(Ct);
    }

    private static TaskItem NewTask(Guid userId, string title, DateOnly due, TaskItemStatus status = TaskItemStatus.Todo) =>
        TaskItem.Create(userId, title, null, due, status, _now);

    [Fact]
    public async Task List_ReturnsOnlyTasksOfGivenUser() // AC-3
    {
        // Arrange
        var ana = await SeedUserAsync("ana@example.com");
        var bob = await SeedUserAsync("bob@example.com");
        await SeedTasksAsync(NewTask(ana.Id, "Ana task", _due), NewTask(bob.Id, "Bob task", _due));
        await using var context = fixture.CreateContext();
        var repository = new TaskRepository(context);

        // Act
        var result = await repository.ListAsync(ana.Id, null, 1, 10, Ct);

        // Assert
        result.Items.Select(t => t.Title).ShouldBe(["Ana task"]);
        result.TotalCount.ShouldBe(1);
    }

    [Fact]
    public async Task GetById_WhenTaskBelongsToAnotherUser_ReturnsNull() // AC-3
    {
        // Arrange
        var ana = await SeedUserAsync("ana@example.com");
        var bob = await SeedUserAsync("bob@example.com");
        var task = NewTask(ana.Id, "Ana task", _due);
        await SeedTasksAsync(task);
        await using var context = fixture.CreateContext();
        var repository = new TaskRepository(context);

        // Act
        var other = await repository.GetByIdAsync(bob.Id, task.Id, Ct);
        var own = await repository.GetByIdAsync(ana.Id, task.Id, Ct);

        // Assert
        other.ShouldBeNull();
        own.ShouldNotBeNull();
        context.Entry(own).State.ShouldBe(EntityState.Unchanged);
    }

    [Fact]
    public async Task List_FiltersByStatusAndOrdersByDueDate() // AC-7
    {
        // Arrange
        var ana = await SeedUserAsync("ana@example.com");
        await SeedTasksAsync(
            NewTask(ana.Id, "Late", new DateOnly(2026, 3, 1), TaskItemStatus.Done),
            NewTask(ana.Id, "Skipped", new DateOnly(2026, 2, 1), TaskItemStatus.Todo),
            NewTask(ana.Id, "Early", new DateOnly(2026, 2, 5), TaskItemStatus.Done));
        await using var context = fixture.CreateContext();
        var repository = new TaskRepository(context);

        // Act
        var result = await repository.ListAsync(ana.Id, TaskItemStatus.Done, 1, 10, Ct);

        // Assert
        result.Items.Select(t => t.Title).ShouldBe(["Early", "Late"]);
        result.TotalCount.ShouldBe(2);
        context.ChangeTracker.Entries().ShouldBeEmpty();
    }

    [Fact]
    public async Task List_ReturnsTotalCountAcrossPages() // AC-7
    {
        // Arrange
        var ana = await SeedUserAsync("ana@example.com");
        await SeedTasksAsync(
            NewTask(ana.Id, "Task one", new DateOnly(2026, 2, 1)),
            NewTask(ana.Id, "Task two", new DateOnly(2026, 2, 2)),
            NewTask(ana.Id, "Task three", new DateOnly(2026, 2, 3)));
        await using var context = fixture.CreateContext();
        var repository = new TaskRepository(context);

        // Act
        var result = await repository.ListAsync(ana.Id, null, 2, 2, Ct);

        // Assert
        result.Items.Select(t => t.Title).ShouldBe(["Task three"]);
        result.TotalCount.ShouldBe(3);
    }

    [Fact]
    public async Task Add_StoresStatusAsText() // AC-6
    {
        // Arrange
        var ana = await SeedUserAsync("ana@example.com");
        var task = NewTask(ana.Id, "Stored task", _due, TaskItemStatus.InProgress);
        await using var context = fixture.CreateContext();
        var repository = new TaskRepository(context);

        // Act
        await repository.AddAsync(task, Ct);

        // Assert
        await using var verify = fixture.CreateContext();
        var status = await verify.Database
            .SqlQuery<string>($"SELECT status AS \"Value\" FROM tasks WHERE id = {task.Id}")
            .SingleAsync(Ct);
        status.ShouldBe("InProgress");
    }

    [Fact]
    public async Task Update_PersistsChangesOfTrackedTask()
    {
        // Arrange
        var ana = await SeedUserAsync("ana@example.com");
        var task = NewTask(ana.Id, "Old title", _due);
        await SeedTasksAsync(task);
        await using var context = fixture.CreateContext();
        var repository = new TaskRepository(context);
        var tracked = (await repository.GetByIdAsync(ana.Id, task.Id, Ct))!;
        tracked.Update("New title", null, _due, _now);

        // Act
        await repository.UpdateAsync(tracked, Ct);

        // Assert
        await using var verify = fixture.CreateContext();
        (await verify.Tasks.SingleAsync(Ct)).Title.ShouldBe("New title");
    }

    [Fact]
    public async Task Delete_RemovesTask()
    {
        // Arrange
        var ana = await SeedUserAsync("ana@example.com");
        var task = NewTask(ana.Id, "Doomed", _due);
        await SeedTasksAsync(task);
        await using var context = fixture.CreateContext();
        var repository = new TaskRepository(context);
        var tracked = (await repository.GetByIdAsync(ana.Id, task.Id, Ct))!;

        // Act
        await repository.DeleteAsync(tracked, Ct);

        // Assert
        await using var verify = fixture.CreateContext();
        (await verify.Tasks.AnyAsync(Ct)).ShouldBeFalse();
    }

    [Fact]
    public async Task DeletingUser_CascadesToTasks()
    {
        // Arrange
        var ana = await SeedUserAsync("ana@example.com");
        await SeedTasksAsync(NewTask(ana.Id, "Ana task", _due));
        await using var context = fixture.CreateContext();

        // Act
        await context.Users.Where(u => u.Id == ana.Id).ExecuteDeleteAsync(Ct);

        // Assert
        (await context.Tasks.AnyAsync(Ct)).ShouldBeFalse();
    }
}
