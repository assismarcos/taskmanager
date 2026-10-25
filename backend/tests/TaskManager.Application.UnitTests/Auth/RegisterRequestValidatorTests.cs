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

    [Fact]
    public void RegisterValidator_WhenPasswordHasNoLetter_ReturnsValidationError()
    {
        // Arrange
        var request = new RegisterRequest("Ana", "ana@example.com", "12345678");

        // Act
        var result = _validator.Validate(request);

        // Assert
        // AC-1
        result.Errors.ShouldContain(failure => failure.PropertyName == nameof(RegisterRequest.Password));
    }

    [Fact]
    public void RegisterValidator_WhenPasswordLetterIsNotAscii_IsValid()
    {
        // Arrange
        var request = new RegisterRequest("Ana", "ana@example.com", "Пароль12");

        // Act
        var result = _validator.Validate(request);

        // Assert
        // AC-1
        result.IsValid.ShouldBeTrue();
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
    [InlineData("ana smith@example.com")]
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
    public void RegisterValidator_WhenEmailIsLongerThan254Chars_ReturnsValidationError()
    {
        // Arrange
        var request = new RegisterRequest("Ana", EmailOfLength(255), "Passw0rd123");

        // Act
        var result = _validator.Validate(request);

        // Assert
        // AC-1
        result.Errors.ShouldContain(failure => failure.PropertyName == nameof(RegisterRequest.Email));
    }

    public static TheoryData<string> InvalidNames => ["", "   ", new string('a', 101)];

    [Theory]
    [MemberData(nameof(InvalidNames))]
    public void RegisterValidator_WhenNameIsBlankOrTooLong_ReturnsValidationError(string name)
    {
        // Arrange
        var request = new RegisterRequest(name, "ana@example.com", "Passw0rd123");

        // Act
        var result = _validator.Validate(request);

        // Assert
        // AC-1
        result.Errors.ShouldContain(failure => failure.PropertyName == nameof(RegisterRequest.Name));
    }

    public static TheoryData<RegisterRequest> RequestsOnTheLimits =>
    [
        new RegisterRequest("Ana", "ana@example.com", "Abcdef12"),
        new RegisterRequest(new string('a', 100), "ana@example.com", "Passw0rd123"),
        new RegisterRequest("Ana", EmailOfLength(254), "Passw0rd123"),
    ];

    [Theory]
    [MemberData(nameof(RequestsOnTheLimits))]
    public void RegisterValidator_WithValuesOnTheLimits_IsValid(RegisterRequest request)
    {
        // Arrange

        // Act
        var result = _validator.Validate(request);

        // Assert
        // AC-1
        result.IsValid.ShouldBeTrue();
    }

    private static string EmailOfLength(int length)
    {
        const string local = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa@";
        const string tld = ".com";
        var label = new string('b', length - local.Length - tld.Length);
        return local + label + tld;
    }
}
