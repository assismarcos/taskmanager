using TaskManager.Application.Tasks;
using TaskManager.Domain.Tasks;

namespace TaskManager.Application.UnitTests.Tasks;

public sealed class TaskListQueryValidatorTests
{
    private readonly TaskListQueryValidator _validator = new();

    [Theory]
    [InlineData(51)]
    [InlineData(0)]
    [InlineData(-1)]
    public void ListQueryValidator_WhenPageSizeIsAbove50_ReturnsValidationError(int pageSize)
    {
        // AC-7
        // Arrange
        var query = new TaskListQuery(null, 1, pageSize);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.PropertyName == nameof(TaskListQuery.PageSize));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10001)]
    [InlineData(int.MaxValue)]
    public void ListQueryValidator_WhenPageIsOutOfRange_ReturnsValidationError(int page)
    {
        // AC-7
        // Arrange
        var query = new TaskListQuery(null, page, 50);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.PropertyName == nameof(TaskListQuery.Page));
    }

    [Fact]
    public void ListQueryValidator_WhenStatusIsNotDefined_ReturnsValidationError()
    {
        // AC-7
        // Arrange
        var query = new TaskListQuery((TaskItemStatus)99);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.Errors.ShouldContain(error => error.PropertyName == nameof(TaskListQuery.Status));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(10000, 50)]
    public void ListQueryValidator_WithBoundaryValues_IsValid(int page, int pageSize)
    {
        // AC-7
        // Arrange
        var query = new TaskListQuery(TaskItemStatus.Todo, page, pageSize);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }
}
