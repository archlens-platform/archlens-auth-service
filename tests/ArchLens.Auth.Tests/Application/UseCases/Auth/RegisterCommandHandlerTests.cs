using ArchLens.Auth.Application.UseCases.Auth.Commands.Register;
using ArchLens.Auth.Domain.Entities.UserEntities;
using ArchLens.Auth.Domain.Interfaces.UserInterfaces;
using ArchLens.Auth.Domain.Interfaces.SecurityInterfaces;
using FluentAssertions;
using NSubstitute;

namespace ArchLens.Auth.Tests.Application.UseCases.Auth;

public class RegisterCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _handler = new RegisterCommandHandler(_userRepository, _passwordHasher);
    }

    [Fact]
    public async Task Handle_ValidData_ShouldCreateUser()
    {
        _userRepository.ExistsByUsernameOrEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _passwordHasher.Hash("StrongPass1").Returns("bcrypt_hash");

        var result = await _handler.Handle(
            new RegisterCommand("newuser", "new@test.com", "StrongPass1", true),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Username.Should().Be("newuser");
        result.Value.Email.Should().Be("new@test.com");
        await _userRepository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateUser_ShouldReturnFailure()
    {
        _userRepository.ExistsByUsernameOrEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(
            new RegisterCommand("existing", "existing@test.com", "StrongPass1", true),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.UserExists");
    }

    [Fact]
    public async Task Handle_ShouldHash_PasswordBeforeStoring()
    {
        _userRepository.ExistsByUsernameOrEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _passwordHasher.Hash("MyPass123").Returns("hashed_value");

        await _handler.Handle(
            new RegisterCommand("user", "user@test.com", "MyPass123", true),
            CancellationToken.None);

        _passwordHasher.Received(1).Hash("MyPass123");
        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(u => u.PasswordHash == "hashed_value"),
            Arg.Any<CancellationToken>());
    }
}
