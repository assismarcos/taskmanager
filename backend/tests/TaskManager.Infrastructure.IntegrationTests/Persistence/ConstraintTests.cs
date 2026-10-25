using Microsoft.EntityFrameworkCore;
using Npgsql;
using TaskManager.Domain.Tasks;
using TaskManager.Domain.Users;
using TaskManager.Infrastructure.IntegrationTests.Fixtures;

namespace TaskManager.Infrastructure.IntegrationTests.Persistence;

[Collection("Postgres")]
[Trait("Category", "Integration")]
public sealed class ConstraintTests(PostgresFixture fixture) : IAsyncLifetime
{
    private static readonly DateTimeOffset _now = new(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);

    public async ValueTask InitializeAsync() => await fixture.ResetAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Users_WithDuplicateEmail_ViolateUniqueIndex() // AC-1
    {
        // Arrange
        await using var context = fixture.CreateContext();
        context.Users.Add(User.Create("Ana", "ana@example.com", "hash", _now));
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        context.Users.Add(User.Create("Ana Two", "ana@example.com", "hash", _now));

        // Act
        var exception = await Should.ThrowAsync<DbUpdateException>(() => context.SaveChangesAsync(TestContext.Current.CancellationToken));

        // Assert
        var postgres = exception.InnerException.ShouldBeOfType<PostgresException>();
        postgres.SqlState.ShouldBe(PostgresErrorCodes.UniqueViolation);
        postgres.ConstraintName.ShouldBe("ix_users_email");
    }

    [Fact]
    public async Task Tasks_WithUnknownUserId_ViolateForeignKey() // AC-3
    {
        // Arrange
        await using var context = fixture.CreateContext();
        var orphan = TaskItem.Create(Guid.NewGuid(), "Orphan task", null, new DateOnly(2026, 2, 1), TaskItemStatus.Todo, _now);
        context.Tasks.Add(orphan);

        // Act
        var exception = await Should.ThrowAsync<DbUpdateException>(() => context.SaveChangesAsync(TestContext.Current.CancellationToken));

        // Assert
        var postgres = exception.InnerException.ShouldBeOfType<PostgresException>();
        postgres.SqlState.ShouldBe(PostgresErrorCodes.ForeignKeyViolation);
    }
}
