using ArchLens.Auth.Application.UseCases.Auth.Queries.ExportUserData;
using ArchLens.Auth.Domain.Entities.UserEntities;
using ArchLens.Auth.Domain.Interfaces.UserInterfaces;
using FluentAssertions;
using NSubstitute;

namespace ArchLens.Auth.Tests.Application.UseCases.Auth;

public class ExportUserDataQueryHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ExportUserDataQueryHandler _handler;

    public ExportUserDataQueryHandlerTests()
    {
        _handler = new ExportUserDataQueryHandler(_userRepository);
    }

    private static User CreateUser()
    {
        var user = User.Create("johndoe", "john@example.com", "hashed_pw", DateTime.UtcNow);
        return user;
    }

    [Fact]
    public async Task Handle_ExistingUser_ShouldReturnExportResponse()
    {
        var user = CreateUser();
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(new ExportUserDataQuery(user.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(user.Id);
        result.Value.Username.Should().Be("johndoe");
        result.Value.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task Handle_ExistingUser_ShouldMapAllFields()
    {
        var consentDate = DateTime.UtcNow.AddDays(-10);
        var user = User.Create("jane", "jane@example.com", "hash", consentDate);
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(new ExportUserDataQuery(user.Id), CancellationToken.None);

        result.Value.Role.Should().Be(UserRole.User.ToString());
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.Value.LgpdConsentGivenAt.Should().Be(consentDate);
    }

    [Fact]
    public async Task Handle_NonExistentUser_ShouldReturnFailure()
    {
        _userRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _handler.Handle(new ExportUserDataQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.UserNotFound");
    }

    [Fact]
    public async Task Handle_ShouldCallRepository_WithCorrectUserId()
    {
        var userId = Guid.NewGuid();
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        await _handler.Handle(new ExportUserDataQuery(userId), CancellationToken.None);

        await _userRepository.Received(1).GetByIdAsync(userId, Arg.Any<CancellationToken>());
    }
}
