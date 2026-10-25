using System.Text;

namespace TaskManager.Application.Auth;

public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt, UserResponse User)
{
    // Keeps the bearer token out of logs and test output.
    private bool PrintMembers(StringBuilder builder)
    {
        builder.Append($"ExpiresAt = {ExpiresAt}, User = {User}");
        return true;
    }
}
