using ArchLens.Auth.Domain.Entities.UserEntities;
using FluentAssertions;

namespace ArchLens.Auth.Tests.Domain.Entities;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var user = User.Create("johndoe", "john@example.com", "hashed_pw", DateTime.UtcNow);

        user.Username.Should().Be("johndoe");
        user.Email.Should().Be("john@example.com");
        user.PasswordHash.Should().Be("hashed_pw");
        user.Role.Should().Be(UserRole.User);
        user.IsActive.Should().BeTrue();
        user.FailedLoginAttempts.Should().Be(0);
        user.LockoutEnd.Should().BeNull();
        user.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_ShouldNormalize_UsernameAndEmail()
    {
        var user = User.Create("  JohnDoe  ", "  John@Example.COM  ", "hash", DateTime.UtcNow);

        user.Username.Should().Be("johndoe");
        user.Email.Should().Be("john@example.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidUsername_ShouldThrow(string? username)
    {
        var act = () => User.Create(username!, "email@test.com", "hash", DateTime.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidEmail_ShouldThrow(string? email)
    {
        var act = () => User.Create("user", email!, "hash", DateTime.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RegisterFailedLogin_ShouldIncrement_Counter()
    {
        var user = User.Create("user", "user@test.com", "hash", DateTime.UtcNow);

        user.RegisterFailedLogin();

        user.FailedLoginAttempts.Should().Be(1);
        user.IsLockedOut().Should().BeFalse();
    }

    [Fact]
    public void RegisterFailedLogin_AfterMaxAttempts_ShouldLockout()
    {
        var user = User.Create("user", "user@test.com", "hash", DateTime.UtcNow);

        for (int i = 0; i < User.MaxFailedAttempts; i++)
            user.RegisterFailedLogin();

        user.IsLockedOut().Should().BeTrue();
        user.LockoutEnd.Should().NotBeNull();
        user.FailedLoginAttempts.Should().Be(User.MaxFailedAttempts);
    }

    [Fact]
    public void RegisterSuccessfulLogin_ShouldReset_FailedAttempts()
    {
        var user = User.Create("user", "user@test.com", "hash", DateTime.UtcNow);
        user.RegisterFailedLogin();
        user.RegisterFailedLogin();

        user.RegisterSuccessfulLogin();

        user.FailedLoginAttempts.Should().Be(0);
        user.LockoutEnd.Should().BeNull();
        user.LastLoginAt.Should().NotBeNull();
    }

    [Fact]
    public void RegisterSuccessfulLogin_AfterLockout_ShouldUnlock()
    {
        var user = User.Create("user", "user@test.com", "hash", DateTime.UtcNow);
        for (int i = 0; i < User.MaxFailedAttempts; i++)
            user.RegisterFailedLogin();

        user.RegisterSuccessfulLogin();

        user.IsLockedOut().Should().BeFalse();
        user.FailedLoginAttempts.Should().Be(0);
    }

    [Fact]
    public void Create_WithAdminRole_ShouldSetRole()
    {
        var user = User.Create("admin", "admin@test.com", "hash", DateTime.UtcNow, UserRole.Admin);

        user.Role.Should().Be(UserRole.Admin);
    }
}
