using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TaskManager.Api.Http;
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
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        var result = await authService.RegisterAsync(request, ct);

        return result.ToActionResult(user => CreatedAtAction(nameof(Me), null, user));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
        => (await authService.LoginAsync(request, ct)).ToActionResult();

    [HttpGet("me")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken ct)
        => (await authService.GetCurrentUserAsync(ct)).ToActionResult();

    [HttpGet("status")]
    [AllowAnonymous]
    [ProducesResponseType<StatusResponse>(StatusCodes.Status200OK)]
    public IActionResult Status() => Ok(new StatusResponse("ok", "1.0.0"));

    public sealed record StatusResponse(string Status, string Version);
}
