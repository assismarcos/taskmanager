using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Auth;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/auth")]
[ApiExplorerSettings(GroupName = "auth-v1")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
        => throw new NotImplementedException(authService.ToString());

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
        => throw new NotImplementedException();

    [HttpGet("me")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public Task<IActionResult> Me(CancellationToken ct)
        => throw new NotImplementedException();

    [HttpGet("status")]
    [AllowAnonymous]
    [ProducesResponseType<StatusResponse>(StatusCodes.Status200OK)]
    public IActionResult Status()
        => throw new NotImplementedException();

    public sealed record StatusResponse(string Status, string Version);
}
