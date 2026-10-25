using TaskManager.Application.Abstractions;

namespace TaskManager.Application.UnitTests.Abstractions;

public sealed class AccessTokenTests
{
    [Fact]
    public void ToString_DoesNotRevealTheToken()
    {
        // Arrange
        var token = new AccessToken("header.payload.signature", new DateTimeOffset(2026, 6, 15, 11, 0, 0, TimeSpan.Zero));

        // Act
        var text = token.ToString();

        // Assert
        text.ShouldNotContain("header.payload.signature");
    }
}
