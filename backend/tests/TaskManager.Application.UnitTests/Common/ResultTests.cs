using TaskManager.Application.Common;

namespace TaskManager.Application.UnitTests.Common;

public sealed class ResultTests
{
    [Fact]
    public void Success_WithValue_IsSuccessAndExposesValue()
    {
        // Act
        var result = Result<int>.Success(42);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(42);
        result.Error.ShouldBeNull();
    }

    [Fact]
    public void Failure_WithError_IsFailureAndExposesError()
    {
        // Arrange
        var error = new Error(ErrorType.NotFound, "task.not_found", "Task not found.");

        // Act
        var result = Result<int>.Failure(error);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(error);
    }

    [Fact]
    public void Value_OnFailure_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = Result<int>.Failure(new Error(ErrorType.NotFound, "task.not_found", "Task not found."));

        // Act
        Func<object?> readValue = () => result.Value;

        // Assert
        readValue.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void ImplicitConversion_FromError_CreatesFailure()
    {
        // Arrange
        var error = new Error(ErrorType.Conflict, "auth.email_taken", "Email is already registered.");

        // Act
        Result<int> result = error;

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(error);
    }
}
