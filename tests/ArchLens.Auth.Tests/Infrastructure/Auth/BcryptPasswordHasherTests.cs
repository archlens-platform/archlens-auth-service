using ArchLens.Auth.Infrastructure.Auth;
using FluentAssertions;

namespace ArchLens.Auth.Tests.Infrastructure.Auth;

public class BcryptPasswordHasherTests
{
    private readonly BcryptPasswordHasher _hasher = new();

    [Fact]
    public void Hash_ShouldReturnNonEmptyString()
    {
        var hash = _hasher.Hash("password123");

        hash.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Hash_ShouldReturnDifferentHashFromPassword()
    {
        var hash = _hasher.Hash("password123");

        hash.Should().NotBe("password123");
    }

    [Fact]
    public void Hash_SamePassword_ShouldReturnDifferentHashes()
    {
        var hash1 = _hasher.Hash("password123");
        var hash2 = _hasher.Hash("password123");

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Verify_CorrectPassword_ShouldReturnTrue()
    {
        var hash = _hasher.Hash("mypassword");

        var result = _hasher.Verify("mypassword", hash);

        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_WrongPassword_ShouldReturnFalse()
    {
        var hash = _hasher.Hash("mypassword");

        var result = _hasher.Verify("wrongpassword", hash);

        result.Should().BeFalse();
    }

    [Fact]
    public void Hash_ShouldProduceBcryptFormat()
    {
        var hash = _hasher.Hash("test");

        hash.Should().StartWith("$2");
    }
}
