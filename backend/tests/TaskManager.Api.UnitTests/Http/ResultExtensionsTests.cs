using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Http;
using TaskManager.Application.Common;

namespace TaskManager.Api.UnitTests.Http;

public sealed class ResultExtensionsTests
{
    [Theory]
    [InlineData(ErrorType.Validation, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ErrorType.Unauthorized, StatusCodes.Status401Unauthorized)]
    public void ToActionResult_ForEachErrorType_ReturnsMatchingStatus(ErrorType type, int expectedStatus)
    {
        // Arrange
        Result<string> result = new Error(type, "some.code", "Something went wrong.");

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = actionResult.ShouldBeAssignableTo<ObjectResult>();
        objectResult!.StatusCode.ShouldBe(expectedStatus);
        var problem = objectResult.Value.ShouldBeAssignableTo<ProblemDetails>();
        problem!.Status.ShouldBe(expectedStatus);
        problem.Detail.ShouldBe("Something went wrong.");
    }

    [Fact]
    public void ToActionResult_ForValidationError_ReturnsValidationProblemWithFieldErrors()
    {
        // Arrange
        var details = new Dictionary<string, string[]> { ["title"] = ["Title is too short."] };
        Result<string> result = new Error(ErrorType.Validation, "validation", "Validation failed.", details);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var problem = actionResult.ShouldBeAssignableTo<ObjectResult>()!.Value.ShouldBeOfType<ValidationProblemDetails>();
        problem.Errors["title"].ShouldBe(["Title is too short."]);
    }

    [Fact]
    public void ToActionResult_ForSuccessfulValue_ReturnsOk()
    {
        // Arrange
        Result<string> result = "value";

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        actionResult.ShouldBeOfType<OkObjectResult>().Value.ShouldBe("value");
    }

    [Fact]
    public void ToActionResult_ForSuccessWithoutValue_ReturnsNoContent()
    {
        // Act
        var actionResult = Result.Success().ToActionResult();

        // Assert
        actionResult.ShouldBeOfType<NoContentResult>();
    }
}
