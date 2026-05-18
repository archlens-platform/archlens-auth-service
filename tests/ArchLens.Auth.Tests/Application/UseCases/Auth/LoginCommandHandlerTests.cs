using ArchLens.Auth.Application.UseCases.Auth.Commands.Login;
using ArchLens.Auth.Application.Contracts.Auth;
using ArchLens.Auth.Domain.Entities.UserEntities;
using ArchLens.Auth.Domain.Interfaces.UserInterfaces;
using ArchLens.Auth.Domain.Interfaces.SecurityInterfaces;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace ArchLens.Auth.Tests.Application.UseCases.Auth;

public class LoginCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _jwtGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly IOptions<JwtOptions> _jwtOptions;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _jwtOptions = Options.Create(new JwtOptions
        {
            Key = "super-secret-key-for-testing-only-32chars!",
            Issuer = "archlens-test",
            Audience = "archlens-test",
            ExpirationMinutes = 60
        });

        _handler = new LoginCommandHandler(_userRepository, _passwordHasher, _jwtGenerator, _jwtOptions);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ShouldReturnToken()
    {
        var user = User.Create("testuser", "test@test.com", "hashed", DateTime.UtcNow);
        _userRepository.GetByUsernameAsync("testuser", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("password123", "hashed").Returns(true);
        _jwtGenerator.Generate(Arg.Any<string>(), "testuser", "User").Returns("jwt-token-here");

        var result = await _handler.Handle(new LoginCommand("testuser", "password123"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().Be("jwt-token-here");
        result.Value.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnFailure()
    {
        _userRepository.GetByUsernameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _handler.Handle(new LoginCommand("nobody", "pass"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
    }

    [Fact]
    public async Task Handle_WrongPassword_ShouldReturnFailure_AndIncrementAttempts()
    {
        var user = User.Create("testuser", "test@test.com", "hashed", DateTime.UtcNow);
        _userRepository.GetByUsernameAsync("testuser", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("wrong", "hashed").Returns(false);

        var result = await _handler.Handle(new LoginCommand("testuser", "wrong"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
        await _userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_LockedAccount_ShouldReturnFailure()
    {
        var user = User.Create("locked", "locked@test.com", "hashed", DateTime.UtcNow);
        for (int i = 0; i < User.MaxFailedAttempts; i++)
            user.RegisterFailedLogin();

        _userRepository.GetByUsernameAsync("locked", Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(new LoginCommand("locked", "pass"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.AccountLocked");
    }

    [Fact]
    public async Task Handle_InactiveAccount_ShouldReturnFailure()
    {
        var user = User.Create("inactive", "inactive@test.com", "hashed", DateTime.UtcNow);
        typeof(User).GetProperty("IsActive")!.SetValue(user, false);

        _userRepository.GetByUsernameAsync("inactive", Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(new LoginCommand("inactive", "pass"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.AccountDisabled");
    }
}
