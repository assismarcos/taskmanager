using TaskManager.Infrastructure.Authentication;

namespace TaskManager.Infrastructure.IntegrationTests.Authentication;

public sealed class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_ThenVerifyWithSamePassword_ReturnsTrue() // AC-1, AC-2
    {
        // Arrange
        var hash = _hasher.Hash("Sample@12345");

        // Act
        var result = _hasher.Verify(hash, "Sample@12345");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse() // AC-2
    {
        // Arrange
        var hash = _hasher.Hash("Sample@12345");

        // Act
        var result = _hasher.Verify(hash, "Wrong@12345");

        // Assert
        result.ShouldBeFalse();
    }
}
