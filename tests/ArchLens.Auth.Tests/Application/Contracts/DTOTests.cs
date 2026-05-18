using ArchLens.Auth.Application.Contracts.DTOs.AuthDTOs;
using ArchLens.Auth.Application.Contracts.Auth;
using FluentAssertions;

namespace ArchLens.Auth.Tests.Application.Contracts;

public class DTOTests
{
    // AuthResponse
    [Fact]
    public void AuthResponse_ShouldStoreAllProperties()
    {
        var response = new AuthResponse("token123", 60, "user1", "Admin");

        response.Token.Should().Be("token123");
        response.ExpiresInMinutes.Should().Be(60);
        response.Username.Should().Be("user1");
        response.Role.Should().Be("Admin");
    }

    [Fact]
    public void AuthResponse_Equality_SameValues_ShouldBeEqual()
    {
        var a = new AuthResponse("token", 60, "user", "User");
        var b = new AuthResponse("token", 60, "user", "User");

        a.Should().Be(b);
    }

    [Fact]
    public void AuthResponse_Equality_DifferentValues_ShouldNotBeEqual()
    {
        var a = new AuthResponse("token1", 60, "user", "User");
        var b = new AuthResponse("token2", 60, "user", "User");

        a.Should().NotBe(b);
    }

    [Fact]
    public void AuthResponse_ToString_ShouldContainProperties()
    {
        var response = new AuthResponse("tok", 30, "usr", "Admin");

        response.ToString().Should().Contain("tok");
    }

    // RegisterResponse
    [Fact]
    public void RegisterResponse_ShouldStoreAllProperties()
    {
        var id = Guid.NewGuid();
        var response = new RegisterResponse(id, "user1", "user1@test.com");

        response.UserId.Should().Be(id);
        response.Username.Should().Be("user1");
        response.Email.Should().Be("user1@test.com");
    }

    [Fact]
    public void RegisterResponse_Equality_SameValues_ShouldBeEqual()
    {
        var id = Guid.NewGuid();
        var a = new RegisterResponse(id, "user", "u@t.com");
        var b = new RegisterResponse(id, "user", "u@t.com");

        a.Should().Be(b);
    }

    [Fact]
    public void RegisterResponse_ToString_ShouldContainProperties()
    {
        var response = new RegisterResponse(Guid.NewGuid(), "user1", "u@t.com");

        response.ToString().Should().Contain("user1");
    }

    // UserDataExportResponse
    [Fact]
    public void UserDataExportResponse_ShouldStoreAllProperties()
    {
        var id = Guid.NewGuid();
        var created = DateTime.UtcNow;
        var login = DateTime.UtcNow.AddHours(-1);
        var consent = DateTime.UtcNow.AddDays(-5);

        var response = new UserDataExportResponse(id, "user1", "u@t.com", "User", created, login, consent);

        response.UserId.Should().Be(id);
        response.Username.Should().Be("user1");
        response.Email.Should().Be("u@t.com");
        response.Role.Should().Be("User");
        response.CreatedAt.Should().Be(created);
        response.LastLoginAt.Should().Be(login);
        response.LgpdConsentGivenAt.Should().Be(consent);
    }

    [Fact]
    public void UserDataExportResponse_WithNullOptionalFields_ShouldWork()
    {
        var response = new UserDataExportResponse(Guid.NewGuid(), "u", "e@t.com", "User", DateTime.UtcNow, null, null);

        response.LastLoginAt.Should().BeNull();
        response.LgpdConsentGivenAt.Should().BeNull();
    }

    [Fact]
    public void UserDataExportResponse_Equality_SameValues_ShouldBeEqual()
    {
        var id = Guid.NewGuid();
        var dt = DateTime.UtcNow;
        var a = new UserDataExportResponse(id, "u", "e@t.com", "User", dt, null, dt);
        var b = new UserDataExportResponse(id, "u", "e@t.com", "User", dt, null, dt);

        a.Should().Be(b);
    }

    // JwtOptions
    [Fact]
    public void JwtOptions_DefaultValues_ShouldBeSet()
    {
        var options = new JwtOptions();

        options.Key.Should().BeEmpty();
        options.Issuer.Should().BeEmpty();
        options.Audience.Should().BeEmpty();
        options.ExpirationMinutes.Should().Be(60);
    }

    [Fact]
    public void JwtOptions_SectionName_ShouldBeJwt()
    {
        JwtOptions.SectionName.Should().Be("Jwt");
    }

    [Fact]
    public void JwtOptions_ShouldAllowSettingProperties()
    {
        var options = new JwtOptions
        {
            Key = "test-key",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpirationMinutes = 120
        };

        options.Key.Should().Be("test-key");
        options.Issuer.Should().Be("test-issuer");
        options.Audience.Should().Be("test-audience");
        options.ExpirationMinutes.Should().Be(120);
    }
}
