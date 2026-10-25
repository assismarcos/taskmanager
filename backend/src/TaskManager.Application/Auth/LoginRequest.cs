using System.Text;

namespace TaskManager.Application.Auth;

public sealed record LoginRequest(string Email, string Password)
{
    // Keeps the password out of logs and test output.
    private bool PrintMembers(StringBuilder builder)
    {
        builder.Append($"Email = {Email}");
        return true;
    }
}
