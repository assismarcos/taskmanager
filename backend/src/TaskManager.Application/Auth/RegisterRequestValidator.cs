using FluentValidation;

namespace TaskManager.Application.Auth;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(request => request.Name).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Email)
            .NotEmpty()
            .MaximumLength(254)
            .Must(email => !email.Trim().Any(char.IsWhiteSpace)).WithMessage("'{PropertyName}' must not contain spaces.")
            .EmailAddress();
        RuleFor(request => request.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches(@"\p{L}").WithMessage("'{PropertyName}' must contain a letter.")
            .Matches("[0-9]").WithMessage("'{PropertyName}' must contain a digit.");
    }
}
