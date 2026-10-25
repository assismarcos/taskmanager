using FluentValidation;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common;
using TaskManager.Domain.Users;

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
    private static readonly Error _invalidCredentials = new(
        ErrorType.Unauthorized, "auth.invalid_credentials", "Invalid email or password");

    private static readonly Error _emailTaken = new(
        ErrorType.Conflict, "auth.email_taken", "Email is already registered.");

    private static readonly Error _invalidToken = new(
        ErrorType.Unauthorized, "auth.invalid_token", "The access token is no longer valid.");

    public async Task<Result<UserResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var validation = await registerValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        var passwordHash = passwordHasher.Hash(request.Password);
        var user = User.Create(request.Name, request.Email, passwordHash, timeProvider.GetUtcNow());

        if (!await users.TryAddAsync(user, ct))
        {
            return _emailTaken;
        }

        return ToResponse(user);
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var validation = await loginValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await users.GetByEmailAsync(normalizedEmail, ct);

        // Same error for an unknown email and a wrong password (AC-2).
        if (user is null || !passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            return _invalidCredentials;
        }

        var token = tokenService.CreateToken(user);
        return new LoginResponse(token.Token, token.ExpiresAt, ToResponse(user));
    }

    public async Task<Result<UserResponse>> GetCurrentUserAsync(CancellationToken ct)
    {
        var user = await users.GetByIdAsync(currentUser.UserId, ct);

        return user is null ? _invalidToken : ToResponse(user);
    }

    private static UserResponse ToResponse(User user) => new(user.Id, user.Name, user.Email);
}
