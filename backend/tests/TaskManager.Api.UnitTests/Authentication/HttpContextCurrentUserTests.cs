using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TaskManager.Api.Authentication;

namespace TaskManager.Api.UnitTests.Authentication;

public sealed class HttpContextCurrentUserTests
{
    [Fact]
    public void HttpContextCurrentUser_ReadsSubClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var identity = new ClaimsIdentity([new Claim("sub", userId.ToString())], "test");
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(new DefaultHttpContext { User = new ClaimsPrincipal(identity) });
        var currentUser = new HttpContextCurrentUser(accessor); // AC-3

        // Act
        var result = currentUser.UserId;

        // Assert
        result.ShouldBe(userId);
    }
}
