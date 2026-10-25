using System.Text;

namespace TaskManager.Application.Abstractions;

public sealed record AccessToken(string Token, DateTimeOffset ExpiresAt)
{
    // Keeps the bearer token out of logs and test output.
    private bool PrintMembers(StringBuilder builder)
    {
        builder.Append($"ExpiresAt = {ExpiresAt}");
        return true;
    }
}
