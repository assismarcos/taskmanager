using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Controllers;
using TaskManager.Application.Auth;
using TaskManager.Application.Common;

namespace TaskManager.Api.UnitTests.Controllers;

public sealed class AuthControllerTests
{
    private static readonly UserResponse _user = new(Guid.NewGuid(), "Ann Lee", "ann@mail.com");

    private readonly IAuthService _authService = Substitute.For<IAuthService>();
    private readonly AuthController _controller;

    public AuthControllerTests() => _controller = new AuthController(_authService);

    [Fact]
    public async Task Register_WhenSuccessful_Returns201WithLocation()
    {
        // Arrange
        var request = new RegisterRequest("Ann Lee", "ann@mail.com", "secret123");
        _authService.RegisterAsync(request, Arg.Any<CancellationToken>()).Returns(_user); // AC-1

        // Act
        var result = await _controller.Register(request, CancellationToken.None);

        // Assert
        var created = result.ShouldBeOfType<CreatedAtActionResult>();
        created.ActionName.ShouldBe(nameof(AuthController.Me));
        created.Value.ShouldBe(_user);
    }

    [Fact]
    public async Task Register_WhenEmailIsTaken_Returns409ProblemDetails()
    {
        // Arrange
        var request = new RegisterRequest("Ann Lee", "ann@mail.com", "secret123");
        _authService.RegisterAsync(request, Arg.Any<CancellationToken>())
            .Returns(new Error(ErrorType.Conflict, "auth.email_taken", "Email is already registered.")); // AC-1

        // Act
        var result = await _controller.Register(request, CancellationToken.None);

        // Assert
        var objectResult = result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(409);
        objectResult.Value.ShouldBeOfType<ProblemDetails>();
    }

    [Fact]
    public async Task Login_WhenSuccessful_Returns200WithToken()
    {
        // Arrange
        var request = new LoginRequest("ann@mail.com", "secret123");
        var response = new LoginResponse("token", DateTimeOffset.UnixEpoch, _user);
        _authService.LoginAsync(request, Arg.Any<CancellationToken>()).Returns(response); // AC-2

        // Act
        var result = await _controller.Login(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }

    [Fact]
    public async Task Login_WhenCredentialsAreInvalid_Returns401ProblemDetails()
    {
        // Arrange
        var request = new LoginRequest("ann@mail.com", "wrong");
        _authService.LoginAsync(request, Arg.Any<CancellationToken>())
            .Returns(new Error(ErrorType.Unauthorized, "auth.invalid_credentials", "Invalid email or password")); // AC-2

        // Act
        var result = await _controller.Login(request, CancellationToken.None);

        // Assert
        var objectResult = result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(401);
        objectResult.Value.ShouldBeOfType<ProblemDetails>().Detail.ShouldBe("Invalid email or password");
    }

    [Fact]
    public async Task Me_ReturnsCurrentUser()
    {
        // Arrange
        _authService.GetCurrentUserAsync(Arg.Any<CancellationToken>()).Returns(_user); // AC-9

        // Act
        var result = await _controller.Me(CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(_user);
    }

    [Fact]
    public void Status_ReturnsOkWithVersion()
    {
        // Act
        var result = _controller.Status();

        // Assert
        var body = result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeOfType<AuthController.StatusResponse>();
        body.Status.ShouldBe("ok");
        body.Version.ShouldNotBeNullOrWhiteSpace();
    }
}
