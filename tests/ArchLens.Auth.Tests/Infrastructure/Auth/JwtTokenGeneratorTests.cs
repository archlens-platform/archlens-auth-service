using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ArchLens.Auth.Application.Contracts.Auth;
using ArchLens.Auth.Infrastructure.Auth;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace ArchLens.Auth.Tests.Infrastructure.Auth;

public class JwtTokenGeneratorTests
{
    private readonly JwtTokenGenerator _generator;
    private readonly JwtOptions _options;

    public JwtTokenGeneratorTests()
    {
        _options = new JwtOptions
        {
            Key = "this-is-a-super-secret-key-for-testing-only-32ch!",
            Issuer = "archlens-test-issuer",
            Audience = "archlens-test-audience",
            ExpirationMinutes = 30
        };
        _generator = new JwtTokenGenerator(Options.Create(_options));
    }

    [Fact]
    public void Generate_ShouldReturnNonEmptyToken()
    {
        var token = _generator.Generate("user-id-123", "testuser", "User");

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Generate_ShouldContainSubClaim()
    {
        var token = _generator.Generate("user-id-123", "testuser", "User");

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "user-id-123");
    }

    [Fact]
    public void Generate_ShouldContainNameClaim()
    {
        var token = _generator.Generate("user-id-123", "testuser", "User");

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "testuser");
    }

    [Fact]
    public void Generate_ShouldContainRoleClaim()
    {
        var token = _generator.Generate("user-id-123", "testuser", "Admin");

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public void Generate_ShouldContainJtiClaim()
    {
        var token = _generator.Generate("user-id-123", "testuser", "User");

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
    }

    [Fact]
    public void Generate_ShouldSetCorrectIssuer()
    {
        var token = _generator.Generate("user-id-123", "testuser", "User");

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Issuer.Should().Be(_options.Issuer);
    }

    [Fact]
    public void Generate_ShouldSetCorrectAudience()
    {
        var token = _generator.Generate("user-id-123", "testuser", "User");

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Audiences.Should().Contain(_options.Audience);
    }

    [Fact]
    public void Generate_ShouldSetExpirationInFuture()
    {
        var token = _generator.Generate("user-id-123", "testuser", "User");

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.ValidTo.Should().BeAfter(DateTime.UtcNow);
        jwt.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void Generate_ShouldProduceUniqueJtiPerCall()
    {
        var token1 = _generator.Generate("user-id-123", "testuser", "User");
        var token2 = _generator.Generate("user-id-123", "testuser", "User");

        var handler = new JwtSecurityTokenHandler();
        var jti1 = handler.ReadJwtToken(token1).Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var jti2 = handler.ReadJwtToken(token2).Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        jti1.Should().NotBe(jti2);
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Admin")]
    public void Generate_WithDifferentRoles_ShouldSetCorrectRoleClaim(string role)
    {
        var token = _generator.Generate("user-id-123", "testuser", role);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == role);
    }

    [Fact]
    public void Generate_WithDifferentExpiration_ShouldReflectInToken()
    {
        var customOptions = new JwtOptions
        {
            Key = "this-is-a-super-secret-key-for-testing-only-32ch!",
            Issuer = "archlens-test-issuer",
            Audience = "archlens-test-audience",
            ExpirationMinutes = 120
        };
        var generator = new JwtTokenGenerator(Options.Create(customOptions));

        var token = generator.Generate("user-id-123", "testuser", "User");

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(120), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void Generate_ShouldUseHmacSha256Algorithm()
    {
        var token = _generator.Generate("user-id-123", "testuser", "User");

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Header.Alg.Should().Be("HS256");
    }

    [Fact]
    public void Generate_TokenShouldBeValidJwt()
    {
        var token = _generator.Generate("user-id-123", "testuser", "User");

        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue();
    }

    [Fact]
    public void Generate_ShouldContainExactlyFourClaims()
    {
        var token = _generator.Generate("user-id-123", "testuser", "User");

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        // sub, jti, name, role (plus standard exp, iss, aud added by framework)
        jwt.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Sub).Should().HaveCount(1);
        jwt.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Jti).Should().HaveCount(1);
        jwt.Claims.Where(c => c.Type == ClaimTypes.Name).Should().HaveCount(1);
        jwt.Claims.Where(c => c.Type == ClaimTypes.Role).Should().HaveCount(1);
    }

    [Fact]
    public void Generate_WithSpecialCharactersInUsername_ShouldSucceed()
    {
        var token = _generator.Generate("user-id-123", "user.name+special@test", "User");

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "user.name+special@test");
    }
}
