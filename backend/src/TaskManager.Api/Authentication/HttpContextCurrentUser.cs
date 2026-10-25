using TaskManager.Application.Abstractions;

namespace TaskManager.Api.Authentication;

public sealed class HttpContextCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid UserId => accessor.HttpContext is null ? Guid.Empty : throw new NotImplementedException();
}
