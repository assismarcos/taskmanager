using Microsoft.Extensions.Time.Testing;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Auth;
using TaskManager.Application.Common;
using TaskManager.Application.UnitTests.Builders;
using TaskManager.Domain.Users;

namespace TaskManager.Application.UnitTests.Auth;

public sealed class AuthServiceTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenService _tokens = Substitute.For<ITokenService>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 3, 1, 8, 0, 0, TimeSpan.Zero));
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _service = new AuthService(
            _users,
            _hasher,
            _tokens,
            _currentUser,
            new RegisterRequestValidator(),
            new LoginRequestValidator(),
            _time);
    }

    [Fact]
    public async Task Register_WithValidRequest_ReturnsUserResponse()
    {
        // Arrange
        _hasher.Hash("Passw0rd123").Returns("hashed");
        _users.TryAddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _service.RegisterAsync(
            new RegisterRequest(" Ana ", " Ana@Example.com ", "Passw0rd123"), CancellationToken.None);

        // Assert
        // AC-1
        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe("Ana");
        result.Value.Email.ShouldBe("ana@example.com");
        result.Value.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Register_WhenEmailIsTaken_ReturnsConflict()
    {
        // Arrange
        _hasher.Hash(Arg.Any<string>()).Returns("hashed");
        _users.TryAddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var result = await _service.RegisterAsync(
            new RegisterRequest("Ana", "ana@example.com", "Passw0rd123"), CancellationToken.None);

        // Assert
        // AC-1
        result.IsSuccess.ShouldBeFalse();
        result.Error.Type.ShouldBe(ErrorType.Conflict);
        result.Error.Code.ShouldBe("auth.email_taken");
    }

    [Fact]
    public async Task Register_HashesPasswordBeforeSaving()
    {
        // Arrange
        _hasher.Hash("Passw0rd123").Returns("hashed");
        _users.TryAddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>()).Returns(true);

        // Act
        await _service.RegisterAsync(
            new RegisterRequest("Ana", "ana@example.com", "Passw0rd123"), CancellationToken.None);

        // Assert
        // AC-1
        await _users.Received(1).TryAddAsync(
            Arg.Is<User>(user => user.PasswordHash == "hashed"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Register_WithInvalidRequest_ReturnsValidationErrorAndSavesNothing()
    {
        // Act
        var result = await _service.RegisterAsync(
            new RegisterRequest("Ana", "bad", "short"), CancellationToken.None);

        // Assert
        // AC-1
        result.IsSuccess.ShouldBeFalse();
        result.Error.Type.ShouldBe(ErrorType.Validation);
        result.Error.Details.ShouldNotBeNull().Keys.ShouldContain("email");
        await _users.DidNotReceive().TryAddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAccessToken()
    {
        // Arrange
        var user = new UserBuilder().Build();
        var expiresAt = _time.GetUtcNow().AddMinutes(60);
        _users.GetByEmailAsync("ana@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _hasher.Verify("stored-hash", "Passw0rd123").Returns(true);
        _tokens.CreateToken(user).Returns(new AccessToken("jwt", expiresAt));

        // Act
        var result = await _service.LoginAsync(
            new LoginRequest("ana@example.com", "Passw0rd123"), CancellationToken.None);

        // Assert
        // AC-2
        result.IsSuccess.ShouldBeTrue();
        result.Value.AccessToken.ShouldBe("jwt");
        result.Value.ExpiresAt.ShouldBe(expiresAt);
        result.Value.User.Id.ShouldBe(user.Id);
    }

    [Fact]
    public async Task Login_WhenEmailIsUnknown_ReturnsInvalidCredentials()
    {
        // Arrange
        _users.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var result = await _service.LoginAsync(
            new LoginRequest("nobody@example.com", "Passw0rd123"), CancellationToken.None);

        // Assert
        // AC-2
        AssertInvalidCredentials(result);
    }

    [Fact]
    public async Task Login_WhenPasswordIsWrong_ReturnsInvalidCredentials()
    {
        // Arrange
        _users.GetByEmailAsync("ana@example.com", Arg.Any<CancellationToken>()).Returns(new UserBuilder().Build());
        _hasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        // Act
        var result = await _service.LoginAsync(
            new LoginRequest("ana@example.com", "WrongPass1"), CancellationToken.None);

        // Assert
        // AC-2
        AssertInvalidCredentials(result);
    }

    [Fact]
    public async Task Login_WithMixedCaseEmail_FindsUser()
    {
        // Arrange
        var user = new UserBuilder().Build();
        _users.GetByEmailAsync("ana@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _hasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        _tokens.CreateToken(user).Returns(new AccessToken("jwt", _time.GetUtcNow()));

        // Act
        var result = await _service.LoginAsync(
            new LoginRequest("  Ana@EXAMPLE.com ", "Passw0rd123"), CancellationToken.None);

        // Assert
        // AC-2
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task Login_WithEmptyFields_ReturnsValidationError()
    {
        // Act
        var result = await _service.LoginAsync(new LoginRequest("", ""), CancellationToken.None);

        // Assert
        // AC-2
        result.IsSuccess.ShouldBeFalse();
        result.Error.Type.ShouldBe(ErrorType.Validation);
        result.Error.Details.ShouldNotBeNull().Keys.ShouldBe(["email", "password"], ignoreOrder: true);
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsUserFromToken()
    {
        // Arrange
        var user = new UserBuilder().Build();
        _currentUser.UserId.Returns(user.Id);
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _service.GetCurrentUserAsync(CancellationToken.None);

        // Assert
        // AC-9
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(new UserResponse(user.Id, user.Name, user.Email));
    }

    [Fact]
    public async Task GetCurrentUser_WhenUserNoLongerExists_ReturnsUnauthorized()
    {
        // Arrange
        _currentUser.UserId.Returns(Guid.NewGuid());
        _users.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var result = await _service.GetCurrentUserAsync(CancellationToken.None);

        // Assert
        // AC-9
        result.IsSuccess.ShouldBeFalse();
        result.Error.Type.ShouldBe(ErrorType.Unauthorized);
    }

    private static void AssertInvalidCredentials(Result<LoginResponse> result)
    {
        result.IsSuccess.ShouldBeFalse();
        result.Error.Type.ShouldBe(ErrorType.Unauthorized);
        result.Error.Code.ShouldBe("auth.invalid_credentials");
        result.Error.Message.ShouldBe("Invalid email or password");
    }
}
