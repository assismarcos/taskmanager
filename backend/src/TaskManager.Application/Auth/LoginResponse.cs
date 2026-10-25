namespace TaskManager.Application.Auth;

public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt, UserResponse User);
