using ArchLens.Auth.Domain.Entities.UserEntities;
using FluentAssertions;

namespace ArchLens.Auth.Tests.Domain.Entities;

public class UserAdditionalTests
{
    [Fact]
    public void PromoteToAdmin_ShouldChangeRoleToAdmin()
    {
        var user = User.Create("user1", "user1@test.com", "hash", DateTime.UtcNow);
        user.Role.Should().Be(UserRole.User);

        user.PromoteToAdmin();

        user.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public void PromoteToAdmin_AlreadyAdmin_ShouldRemainAdmin()
    {
        var user = User.Create("admin", "admin@test.com", "hash", DateTime.UtcNow, UserRole.Admin);

        user.PromoteToAdmin();

        user.Role.Should().Be(UserRole.Admin);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidPasswordHash_ShouldThrow(string? hash)
    {
        var act = () => User.Create("user", "user@test.com", hash!, DateTime.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IsLockedOut_WhenNotLocked_ShouldReturnFalse()
    {
        var user = User.Create("user", "user@test.com", "hash", DateTime.UtcNow);

        user.IsLockedOut().Should().BeFalse();
    }

    [Fact]
    public void IsLockedOut_WhenLockoutEndInPast_ShouldReturnFalse()
    {
        var user = User.Create("user", "user@test.com", "hash", DateTime.UtcNow);
        for (int i = 0; i < User.MaxFailedAttempts; i++)
            user.RegisterFailedLogin();

        // Set LockoutEnd to the past via reflection
        typeof(User).GetProperty("LockoutEnd")!.SetValue(user, DateTime.UtcNow.AddMinutes(-1));

        user.IsLockedOut().Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldSetLgpdConsentGivenAt()
    {
        var consentDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var user = User.Create("user", "user@test.com", "hash", consentDate);

        user.LgpdConsentGivenAt.Should().Be(consentDate);
    }

    [Fact]
    public void Create_ShouldSetCreatedAtToUtcNow()
    {
        var before = DateTime.UtcNow;
        var user = User.Create("user", "user@test.com", "hash", DateTime.UtcNow);
        var after = DateTime.UtcNow;

        user.CreatedAt.Should().BeOnOrAfter(before);
        user.CreatedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        var user1 = User.Create("user1", "u1@test.com", "hash1", DateTime.UtcNow);
        var user2 = User.Create("user2", "u2@test.com", "hash2", DateTime.UtcNow);

        user1.Id.Should().NotBe(user2.Id);
    }

    [Fact]
    public void RegisterSuccessfulLogin_ShouldSetLastLoginAt()
    {
        var user = User.Create("user", "user@test.com", "hash", DateTime.UtcNow);
        user.LastLoginAt.Should().BeNull();

        user.RegisterSuccessfulLogin();

        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void RegisterFailedLogin_BelowMax_ShouldNotSetLockout()
    {
        var user = User.Create("user", "user@test.com", "hash", DateTime.UtcNow);

        for (int i = 0; i < User.MaxFailedAttempts - 1; i++)
            user.RegisterFailedLogin();

        user.LockoutEnd.Should().BeNull();
        user.FailedLoginAttempts.Should().Be(User.MaxFailedAttempts - 1);
    }

    [Fact]
    public void MaxFailedAttempts_ShouldBe5()
    {
        User.MaxFailedAttempts.Should().Be(5);
    }

    [Fact]
    public void LockoutMinutes_ShouldBe15()
    {
        User.LockoutMinutes.Should().Be(15);
    }
}
