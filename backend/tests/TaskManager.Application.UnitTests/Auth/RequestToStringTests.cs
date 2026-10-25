using TaskManager.Application.Auth;

namespace TaskManager.Application.UnitTests.Auth;

public sealed class RequestToStringTests
{
    [Fact]
    public void RegisterRequest_ToString_DoesNotRevealThePassword()
    {
        // Arrange
        var request = new RegisterRequest("Ana", "ana@example.com", "Secr3tPassword");

        // Act
        var text = request.ToString();

        // Assert
        text.ShouldNotContain("Secr3tPassword");
    }

    [Fact]
    public void LoginRequest_ToString_DoesNotRevealThePassword()
    {
        // Arrange
        var request = new LoginRequest("ana@example.com", "Secr3tPassword");

        // Act
        var text = request.ToString();

        // Assert
        text.ShouldNotContain("Secr3tPassword");
    }

    [Fact]
    public void LoginResponse_ToString_DoesNotRevealTheToken()
    {
        // Arrange
        var user = new UserResponse(Guid.NewGuid(), "Ana", "ana@example.com");
        var response = new LoginResponse("Secr3tToken", DateTimeOffset.UnixEpoch, user);

        // Act
        var text = response.ToString();

        // Assert
        text.ShouldNotContain("Secr3tToken");
    }
}
