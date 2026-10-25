using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using TaskManager.Domain.Tasks;
using TaskManager.Infrastructure.Authentication;
using TaskManager.Infrastructure.IntegrationTests.Fixtures;
using TaskManager.Infrastructure.Persistence.Seed;

namespace TaskManager.Infrastructure.IntegrationTests.Seed;

[Collection("Postgres")]
[Trait("Category", "Integration")]
public sealed class DatabaseInitializerTests(PostgresFixture fixture) : IAsyncLifetime
{
    private const string DemoEmail = "demo@taskmanager.local";
    private const string OtherEmail = "other@taskmanager.local";

    private static readonly DateTimeOffset _now = new(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);

    private readonly FakeTimeProvider _time = new(_now);

    public async ValueTask InitializeAsync() => await fixture.ResetAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private async Task RunSeedAsync()
    {
        await using var context = fixture.CreateContext();
        var initializer = new DatabaseInitializer(context, new PasswordHasher(), _time);
        await initializer.InitializeAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Seed_OnEmptyDatabase_CreatesDemoUsersAndTasks()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;

        // Act
        await RunSeedAsync();

        // Assert
        await using var context = fixture.CreateContext();
        var users = await context.Users.ToListAsync(ct);
        users.Select(u => u.Email).ShouldBe([DemoEmail, OtherEmail], ignoreOrder: true);
        var demo = users.Single(u => u.Email == DemoEmail);
        var other = users.Single(u => u.Email == OtherEmail);
        new PasswordHasher().Verify(demo.PasswordHash, "Demo@12345").ShouldBeTrue();
        new PasswordHasher().Verify(other.PasswordHash, "Other@12345").ShouldBeTrue();
        (await context.Tasks.CountAsync(t => t.UserId == demo.Id, ct)).ShouldBe(8);
        (await context.Tasks.CountAsync(t => t.UserId == other.Id, ct)).ShouldBe(2);
    }

    [Fact]
    public async Task Seed_RunTwice_DoesNotDuplicateData()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        await RunSeedAsync();

        // Act
        await RunSeedAsync();

        // Assert
        await using var context = fixture.CreateContext();
        (await context.Users.CountAsync(ct)).ShouldBe(2);
        (await context.Tasks.CountAsync(ct)).ShouldBe(10);
    }

    [Fact]
    public async Task Seed_CreatesAtLeastOneOverdueTaskForDemoUser()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var today = DateOnly.FromDateTime(_now.UtcDateTime);

        // Act
        await RunSeedAsync();

        // Assert
        await using var context = fixture.CreateContext();
        var demo = await context.Users.SingleAsync(u => u.Email == DemoEmail, ct);
        var tasks = await context.Tasks.Where(t => t.UserId == demo.Id).ToListAsync(ct);
        tasks.Count(t => t.IsOverdue(today)).ShouldBeGreaterThanOrEqualTo(1);
        tasks.Count(t => t.Status == TaskItemStatus.Todo).ShouldBe(4);
        tasks.Count(t => t.Status == TaskItemStatus.InProgress).ShouldBe(2);
        tasks.Count(t => t.Status == TaskItemStatus.Done).ShouldBe(2);
    }
}
