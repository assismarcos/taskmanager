using System.ComponentModel.DataAnnotations;
using TaskManager.Infrastructure.Authentication;

namespace TaskManager.Infrastructure.IntegrationTests.Authentication;

public sealed class JwtOptionsTests
{
    [Fact]
    public void JwtOptions_WithKeyShorterThan32Chars_FailsValidation()
    {
        // Arrange
        var options = new JwtOptions { Issuer = "issuer", Audience = "audience", Key = new string('k', 31) };
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(options, new ValidationContext(options), results, validateAllProperties: true);

        // Assert
        isValid.ShouldBeFalse();
        results.ShouldContain(r => r.MemberNames.Contains(nameof(JwtOptions.Key)));
    }

    [Fact]
    public void JwtOptions_WithKeyOfExactly32Chars_PassesValidation()
    {
        // Arrange
        var options = new JwtOptions { Issuer = "issuer", Audience = "audience", Key = new string('k', 32) };

        // Act
        var isValid = Validator.TryValidateObject(options, new ValidationContext(options), [], validateAllProperties: true);

        // Assert
        isValid.ShouldBeTrue();
    }
}
