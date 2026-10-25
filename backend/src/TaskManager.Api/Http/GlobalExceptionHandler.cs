using Microsoft.AspNetCore.Diagnostics;

namespace TaskManager.Api.Http;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        { _ = logger; throw new NotImplementedException(); }
}
