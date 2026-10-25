using TaskManager.Application.Auth;

namespace TaskManager.Application.UnitTests.Auth;

public sealed class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public void RegisterValidator_WhenPasswordHasNoDigit_ReturnsValidationError()
    {
        // Arrange
        var request = new RegisterRequest("Ana", "ana@example.com", "NoDigitsHere");

        // Act
        var result = _validator.Validate(request);

        // Assert
        // AC-1
        result.Errors.ShouldContain(failure => failure.PropertyName == nameof(RegisterRequest.Password));
    }

    [Theory]
    [InlineData("Ab1")]
    [InlineData("Abcdef1")]
    public void RegisterValidator_WhenPasswordIsShorterThan8Chars_ReturnsValidationError(string password)
    {
        // Arrange
        var request = new RegisterRequest("Ana", "ana@example.com", password);

        // Act
        var result = _validator.Validate(request);

        // Assert
        // AC-1
        result.Errors.ShouldContain(failure => failure.PropertyName == nameof(RegisterRequest.Password));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void RegisterValidator_WhenEmailIsInvalid_ReturnsValidationError(string email)
    {
        // Arrange
        var request = new RegisterRequest("Ana", email, "Passw0rd123");

        // Act
        var result = _validator.Validate(request);

        // Assert
        // AC-1
        result.Errors.ShouldContain(failure => failure.PropertyName == nameof(RegisterRequest.Email));
    }

    [Fact]
    public void RegisterValidator_WithValidRequest_IsValid()
    {
        // Arrange
        var request = new RegisterRequest("Ana", "ana@example.com", "Passw0rd123");

        // Act
        var result = _validator.Validate(request);

        // Assert
        // AC-1
        result.IsValid.ShouldBeTrue();
    }
}
