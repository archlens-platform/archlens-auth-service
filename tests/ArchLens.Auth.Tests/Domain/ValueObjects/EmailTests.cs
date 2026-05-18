using ArchLens.Auth.Domain.ValueObjects.Users;
using FluentAssertions;

namespace ArchLens.Auth.Tests.Domain.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("john@example.com", "john@example.com")]
    [InlineData("  JOHN@EXAMPLE.COM  ", "john@example.com")]
    [InlineData("User.Name+tag@Sub.Domain.com", "user.name+tag@sub.domain.com")]
    public void Create_ValidEmail_ShouldNormalizeToLowercase(string input, string expected)
    {
        var email = Email.Create(input);

        email.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyOrWhitespace_ShouldThrow(string input)
    {
        var act = () => Email.Create(input);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_NullEmail_ShouldThrow()
    {
        var act = () => Email.Create(null!);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing-at-sign.com")]
    [InlineData("@nodomain")]
    public void Create_InvalidFormat_ShouldThrow(string input)
    {
        var act = () => Email.Create(input);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid email format*");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var email = Email.Create("user@example.com");

        email.ToString().Should().Be("user@example.com");
    }

    [Fact]
    public void ImplicitString_ShouldReturnValue()
    {
        var email = Email.Create("user@example.com");

        string value = email;

        value.Should().Be("user@example.com");
    }

    [Fact]
    public void TwoEmails_SamValue_ShouldBeEqual()
    {
        var a = Email.Create("user@example.com");
        var b = Email.Create("USER@EXAMPLE.COM");

        a.Should().Be(b);
    }

    [Fact]
    public void TwoEmails_DifferentValues_ShouldNotBeEqual()
    {
        var a = Email.Create("alice@example.com");
        var b = Email.Create("bob@example.com");

        a.Should().NotBe(b);
    }
}
