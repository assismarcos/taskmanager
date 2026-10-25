using FluentValidation.Results;
using TaskManager.Application.Common;

namespace TaskManager.Application.UnitTests.Common;

public sealed class ValidationResultExtensionsTests
{
    [Fact]
    public void ToError_WithFailures_ReturnsValidationErrorWithCamelCaseFieldKeys()
    {
        // Arrange
        var result = new ValidationResult(
        [
            new ValidationFailure("Email", "Email is invalid."),
            new ValidationFailure("Password", "Too short."),
            new ValidationFailure("Password", "Needs a digit."),
        ]);

        // Act
        var error = result.ToError();

        // Assert
        error.Type.ShouldBe(ErrorType.Validation);
        error.Code.ShouldBe("validation");
        error.Details.ShouldNotBeNull();
        error.Details.Keys.ShouldBe(["email", "password"], ignoreOrder: true);
        error.Details["email"].ShouldBe(["Email is invalid."]);
        error.Details["password"].ShouldBe(["Too short.", "Needs a digit."]);
    }

    [Theory]
    [InlineData("DueDate", "dueDate")]
    [InlineData("Title", "title")]
    [InlineData("Items[0].DueDate", "items[0].dueDate")]
    public void ToError_WithPascalCaseProperty_ReturnsCamelCaseKey(string property, string expectedKey)
    {
        // Arrange
        var result = new ValidationResult([new ValidationFailure(property, "Invalid.")]);

        // Act
        var error = result.ToError();

        // Assert
        error.Details.ShouldNotBeNull().Keys.ShouldBe([expectedKey]);
    }
}
