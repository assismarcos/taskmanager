using TaskManager.Domain.Common;
using TaskManager.Domain.Users;

namespace TaskManager.Domain.UnitTests.Users;

public sealed class UserTests
{
    private static readonly DateTimeOffset _now = new(2026, 6, 15, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithMixedCaseEmail_StoresLowercaseEmail()
    {
        // AC-1
        // Arrange
        const string email = "  Ada.Lovelace@Example.COM ";

        // Act
        var user = User.Create("Ada Lovelace", email, "hash", _now);

        // Assert
        user.Email.ShouldBe("ada.lovelace@example.com");
        user.Name.ShouldBe("Ada Lovelace");
        user.PasswordHash.ShouldBe("hash");
        user.CreatedAt.ShouldBe(_now);
        user.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void Create_WhenNameIsEmpty_ThrowsDomainException()
    {
        // AC-1
        // Arrange
        const string name = "  ";

        // Act
        var act = () => User.Create(name, "ada@example.com", "hash", _now);

        // Assert
        act.ShouldThrow<DomainException>();
    }

    [Fact]
    public void Create_WhenNameIsLongerThan100Chars_ThrowsDomainException()
    {
        // AC-1
        // Arrange
        var name = new string('a', 101);

        // Act
        var act = () => User.Create(name, "ada@example.com", "hash", _now);

        // Assert
        act.ShouldThrow<DomainException>();
    }
}
