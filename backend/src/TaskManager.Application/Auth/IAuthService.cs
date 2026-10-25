using TaskManager.Application.Common;

namespace TaskManager.Application.Auth;

public interface IAuthService
{
    Task<Result<UserResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct);

    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct);

    Task<Result<UserResponse>> GetCurrentUserAsync(CancellationToken ct);
}
