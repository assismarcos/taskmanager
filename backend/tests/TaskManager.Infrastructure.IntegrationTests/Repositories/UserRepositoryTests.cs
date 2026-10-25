using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Users;
using TaskManager.Infrastructure.IntegrationTests.Fixtures;
using TaskManager.Infrastructure.Persistence.Repositories;

namespace TaskManager.Infrastructure.IntegrationTests.Repositories;

[Collection("Postgres")]
[Trait("Category", "Integration")]
public sealed class UserRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    private static readonly DateTimeOffset _now = new(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);

    public async ValueTask InitializeAsync() => await fixture.ResetAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task TryAdd_WhenEmailIsTaken_ReturnsFalse() // AC-1
    {
        // Arrange
        await using var context = fixture.CreateContext();
        var repository = new UserRepository(context);
        (await repository.TryAddAsync(User.Create("Ana", "ana@example.com", "hash", _now), Ct)).ShouldBeTrue();
        var duplicate = User.Create("Ana Two", "ana@example.com", "hash", _now);

        // Act
        var added = await repository.TryAddAsync(duplicate, Ct);

        // Assert
        added.ShouldBeFalse();
        context.Entry(duplicate).State.ShouldBe(EntityState.Detached);
        (await context.Users.CountAsync(Ct)).ShouldBe(1);
    }

    [Fact]
    public async Task GetByEmail_ReturnsMatchingUser() // AC-1
    {
        // Arrange
        await using var seed = fixture.CreateContext();
        var ana = User.Create("Ana", "ana@example.com", "hash", _now);
        seed.Users.Add(ana);
        seed.Users.Add(User.Create("Bob", "bob@example.com", "hash", _now));
        await seed.SaveChangesAsync(Ct);
        await using var context = fixture.CreateContext();
        var repository = new UserRepository(context);

        // Act
        var found = await repository.GetByEmailAsync("ana@example.com", Ct);
        var missing = await repository.GetByEmailAsync("nobody@example.com", Ct);

        // Assert
        found.ShouldNotBeNull().Id.ShouldBe(ana.Id);
        missing.ShouldBeNull();
    }

    [Fact]
    public async Task GetById_ReturnsMatchingUser()
    {
        // Arrange
        await using var seed = fixture.CreateContext();
        var ana = User.Create("Ana", "ana@example.com", "hash", _now);
        seed.Users.Add(ana);
        await seed.SaveChangesAsync(Ct);
        await using var context = fixture.CreateContext();
        var repository = new UserRepository(context);

        // Act
        var found = await repository.GetByIdAsync(ana.Id, Ct);
        var missing = await repository.GetByIdAsync(Guid.NewGuid(), Ct);

        // Assert
        found.ShouldNotBeNull().Email.ShouldBe("ana@example.com");
        missing.ShouldBeNull();
    }
}
