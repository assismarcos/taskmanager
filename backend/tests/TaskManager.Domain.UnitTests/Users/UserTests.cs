using TaskManager.Domain.Common;
using TaskManager.Domain.Users;

namespace TaskManager.Domain.UnitTests.Users;

public class UserTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 15, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithMixedCaseEmail_StoresLowercaseEmail()
    {
        // AC-1
        var user = User.Create("Ada Lovelace", "  Ada.Lovelace@Example.COM ", "hash", Now);

        user.Email.ShouldBe("ada.lovelace@example.com");
        user.Name.ShouldBe("Ada Lovelace");
        user.PasswordHash.ShouldBe("hash");
        user.CreatedAt.ShouldBe(Now);
        user.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void Create_WhenNameIsEmpty_ThrowsDomainException()
    {
        // AC-1
        Should.Throw<DomainException>(() => User.Create("  ", "ada@example.com", "hash", Now));
    }

    [Fact]
    public void Create_WhenNameIsLongerThan100Chars_ThrowsDomainException()
    {
        // AC-1
        Should.Throw<DomainException>(() => User.Create(new string('a', 101), "ada@example.com", "hash", Now));
    }
}
