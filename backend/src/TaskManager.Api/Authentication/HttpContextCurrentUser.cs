using TaskManager.Application.Abstractions;

namespace TaskManager.Api.Authentication;

public sealed class HttpContextCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid UserId
    {
        get
        {
            var sub = accessor.HttpContext?.User.FindFirst("sub")?.Value;

            return Guid.TryParse(sub, out var id)
                ? id
                : throw new InvalidOperationException("The request has no valid 'sub' claim.");
        }
    }
}
