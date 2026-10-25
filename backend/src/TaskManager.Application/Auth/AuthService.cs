using FluentValidation;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common;

namespace TaskManager.Application.Auth;

public sealed class AuthService(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    ICurrentUser currentUser,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator,
    TimeProvider timeProvider) : IAuthService
{
    private readonly object[] _unused =
        [users, passwordHasher, tokenService, currentUser, registerValidator, loginValidator, timeProvider];

    public Task<Result<UserResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct) =>
        throw new NotImplementedException();

    public Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct) =>
        throw new NotImplementedException();

    public Task<Result<UserResponse>> GetCurrentUserAsync(CancellationToken ct) =>
        throw new NotImplementedException();
}
