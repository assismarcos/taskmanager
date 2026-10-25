using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using TaskManager.Domain.Users;
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
    public void Verify_WithHashThatNeedsRehash_ReturnsTrue() // AC-2
    {
        // Arrange
        var legacy = new PasswordHasher<User>(Options.Create(new PasswordHasherOptions
        {
            CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2,
        }));
        var hash = legacy.HashPassword(null!, "Sample@12345");

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
