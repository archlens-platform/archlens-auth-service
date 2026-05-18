using ArchLens.Auth.Application.UseCases.Auth.Commands.DeleteAccount;
using ArchLens.Auth.Domain.Entities.UserEntities;
using ArchLens.Auth.Domain.Interfaces.UserInterfaces;
using ArchLens.Contracts.Events;
using FluentAssertions;
using MassTransit;
using NSubstitute;

namespace ArchLens.Auth.Tests.Application.UseCases.Auth;

public class DeleteAccountCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPublishEndpoint _publishEndpoint = Substitute.For<IPublishEndpoint>();
    private readonly DeleteAccountCommandHandler _handler;

    public DeleteAccountCommandHandlerTests()
    {
        _handler = new DeleteAccountCommandHandler(_userRepository, _publishEndpoint);
    }

    private static User CreateUser() =>
        User.Create("johndoe", "john@example.com", "hashed_pw", DateTime.UtcNow);

    [Fact]
    public async Task Handle_ExistingUser_ShouldDeleteAndPublishEvent()
    {
        var user = CreateUser();
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(new DeleteAccountCommand(user.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        await _userRepository.Received(1).DeleteAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingUser_ShouldPublishUserAccountDeletedEvent()
    {
        var user = CreateUser();
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        await _handler.Handle(new DeleteAccountCommand(user.Id), CancellationToken.None);

        await _publishEndpoint.Received(1).Publish(
            Arg.Is<UserAccountDeletedEvent>(e => e.UserId == user.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NonExistentUser_ShouldReturnFailure()
    {
        _userRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _handler.Handle(
            new DeleteAccountCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.UserNotFound");
    }

    [Fact]
    public async Task Handle_NonExistentUser_ShouldNotDeleteOrPublish()
    {
        _userRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        await _handler.Handle(new DeleteAccountCommand(Guid.NewGuid()), CancellationToken.None);

        await _userRepository.DidNotReceive().DeleteAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _publishEndpoint.DidNotReceive().Publish(
            Arg.Any<UserAccountDeletedEvent>(), Arg.Any<CancellationToken>());
    }
}
