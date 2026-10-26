using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Abstractions;
using TaskManager.Domain.Tasks;
using TaskManager.Domain.Users;

namespace TaskManager.Infrastructure.Persistence.Seed;

public sealed class DatabaseInitializer(AppDbContext context, IPasswordHasher passwordHasher, TimeProvider timeProvider)
{
    private const string DemoEmail = "demo@taskmanager.local";
    private const string OtherEmail = "other@taskmanager.local";

    public async Task InitializeAsync(CancellationToken ct)
    {
        await context.Database.MigrateAsync(ct);

        if (await context.Users.AnyAsync(u => u.Email == DemoEmail, ct))
        {
            return;
        }

        var now = timeProvider.GetUtcNow();
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        var seededAt = now.AddDays(-30);

        // Only the documented demo credentials (DD-04 section 4).
        var demo = User.Create("Demo User", DemoEmail, passwordHasher.Hash("Demo@12345"), seededAt);
        var other = User.Create("Other User", OtherEmail, passwordHasher.Hash("Other@12345"), seededAt);
        context.Users.AddRange(demo, other);

        context.Tasks.AddRange(_demoTasks.Select(task => BuildTask(demo, task, today, seededAt)));
        context.Tasks.AddRange(_otherTasks.Select(task => BuildTask(other, task, today, seededAt)));

        await context.SaveChangesAsync(ct);
    }

    // Created 30 days back so past due dates respect the domain invariant (DD-04 section 4).
    private static TaskItem BuildTask(User owner, SeedTask task, DateOnly today, DateTimeOffset seededAt) =>
        TaskItem.Create(owner.Id, task.Title, null, today.AddDays(task.DueInDays), task.Status, seededAt);

    private sealed record SeedTask(string Title, int DueInDays, TaskItemStatus Status);

    private static readonly SeedTask[] _demoTasks =
    [
        new("Prepare sprint planning", 2, TaskItemStatus.Todo),
        new("Review pull requests", 5, TaskItemStatus.Todo),
        new("Submit expense report", -3, TaskItemStatus.Todo),
        new("Plan the product roadmap", 180, TaskItemStatus.Todo),
        new("Refactor the billing module", 7, TaskItemStatus.Todo),
        new("Update project dependencies", 9, TaskItemStatus.Todo),
        new("Draft the hiring plan", 14, TaskItemStatus.Todo),
        new("Write API documentation", 4, TaskItemStatus.InProgress),
        new("Fix login redirect bug", -2, TaskItemStatus.InProgress),
        new("Prepare the demo environment", 3, TaskItemStatus.InProgress),
        new("Write tests for the reports", 6, TaskItemStatus.InProgress),
        new("Set up CI pipeline", 1, TaskItemStatus.Done),
        new("Update team onboarding guide", -5, TaskItemStatus.Done),
        new("Close the sprint retrospective", 2, TaskItemStatus.Done),
        new("Archive old feature flags", 8, TaskItemStatus.Done),
    ];

    private static readonly SeedTask[] _otherTasks =
    [
        new("Book the team offsite", 10, TaskItemStatus.Todo),
        new("Update the vendor contract", 1, TaskItemStatus.Todo),
        new("Prepare the quarterly budget", 6, TaskItemStatus.Todo),
        new("Organize the knowledge base", 12, TaskItemStatus.Todo),
        new("Plan the security audit", 15, TaskItemStatus.Todo),
        new("Order new laptops", -4, TaskItemStatus.Todo),
        new("Migrate the wiki", 20, TaskItemStatus.Todo),
        new("Prepare the all-hands slides", 8, TaskItemStatus.Todo),
        new("Update the disaster recovery plan", 25, TaskItemStatus.Todo),
        new("Renew the domain names", 30, TaskItemStatus.Todo),
        new("Renew software licenses", 3, TaskItemStatus.InProgress),
        new("Interview frontend candidates", 4, TaskItemStatus.InProgress),
        new("Clean up old cloud resources", 7, TaskItemStatus.InProgress),
        new("Write the release notes", 2, TaskItemStatus.InProgress),
        new("Review the incident report", -1, TaskItemStatus.InProgress),
        new("Set up the staging database", 9, TaskItemStatus.InProgress),
        new("Run the accessibility review", 14, TaskItemStatus.InProgress),
        new("Schedule one-on-ones", 5, TaskItemStatus.Done),
        new("Archive last year's projects", -6, TaskItemStatus.Done),
        new("Send the customer survey", 11, TaskItemStatus.Done),
    ];
}
