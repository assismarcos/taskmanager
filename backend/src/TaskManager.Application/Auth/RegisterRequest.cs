using System.Text;

namespace TaskManager.Application.Auth;

public sealed record RegisterRequest(string Name, string Email, string Password)
{
    // Keeps the password out of logs and test output.
    private bool PrintMembers(StringBuilder builder)
    {
        builder.Append($"Name = {Name}, Email = {Email}");
        return true;
    }
}
