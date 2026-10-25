using FluentValidation;

namespace TaskManager.Application.Auth;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Email).NotEmpty();
        RuleFor(request => request.Password).NotEmpty();
    }
}
