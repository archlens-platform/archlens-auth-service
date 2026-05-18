using System.Security.Claims;
using ArchLens.Auth.Api.Controllers;
using ArchLens.Auth.Application.Contracts.DTOs.AuthDTOs;
using ArchLens.Auth.Application.UseCases.Auth.Commands.DeleteAccount;
using ArchLens.Auth.Application.UseCases.Auth.Commands.Login;
using ArchLens.Auth.Application.UseCases.Auth.Commands.Register;
using ArchLens.Auth.Application.UseCases.Auth.Queries.ExportUserData;
using ArchLens.Auth.Domain.Entities.UserEntities;
using ArchLens.Auth.Domain.Interfaces.UserInterfaces;
using ArchLens.SharedKernel.Application;
using ArchLens.SharedKernel.Domain;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace ArchLens.Auth.Tests.Api.Controllers;

public class AuthControllerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _controller = new AuthController(_mediator, _userRepository, _unitOfWork);
    }

    private void SetUser(Guid userId)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    private void SetAnonymousUser()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    // Register

    [Fact]
    public async Task Register_Success_ShouldReturnCreated()
    {
        var command = new RegisterCommand("user1", "user@test.com", "Pass1234", true);
        var response = new RegisterResponse(Guid.NewGuid(), "user1", "user@test.com");
        _mediator.Send(command, Arg.Any<CancellationToken>())
            .Returns(Result.Success(response));

        var result = await _controller.Register(command, CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Register_Failure_ShouldReturnBadRequest()
    {
        var command = new RegisterCommand("user1", "user@test.com", "Pass1234", true);
        _mediator.Send(command, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<RegisterResponse>(new Error("Auth.UserExists", "Already exists")));

        var result = await _controller.Register(command, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // Login

    [Fact]
    public async Task Login_Success_ShouldReturnOk()
    {
        var command = new LoginCommand("user1", "Pass1234");
        var response = new AuthResponse("token", 60, "user1", "User");
        _mediator.Send(command, Arg.Any<CancellationToken>())
            .Returns(Result.Success(response));

        var result = await _controller.Login(command, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Login_Failure_ShouldReturnUnauthorized()
    {
        var command = new LoginCommand("user1", "wrong");
        _mediator.Send(command, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<AuthResponse>(new Error("Auth.InvalidCredentials", "Invalid")));

        var result = await _controller.Login(command, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    // ExportMyData

    [Fact]
    public async Task ExportMyData_NoUser_ShouldReturnUnauthorized()
    {
        SetAnonymousUser();

        var result = await _controller.ExportMyData(CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task ExportMyData_Success_ShouldReturnOk()
    {
        var userId = Guid.NewGuid();
        SetUser(userId);
        var response = new UserDataExportResponse(userId, "user1", "u@t.com", "User", DateTime.UtcNow, null, DateTime.UtcNow);
        _mediator.Send(Arg.Any<ExportUserDataQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(response));

        var result = await _controller.ExportMyData(CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ExportMyData_NotFound_ShouldReturnNotFound()
    {
        var userId = Guid.NewGuid();
        SetUser(userId);
        _mediator.Send(Arg.Any<ExportUserDataQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<UserDataExportResponse>(new Error("Auth.UserNotFound", "Not found")));

        var result = await _controller.ExportMyData(CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // DeleteMyAccount

    [Fact]
    public async Task DeleteMyAccount_NoUser_ShouldReturnUnauthorized()
    {
        SetAnonymousUser();

        var result = await _controller.DeleteMyAccount(CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task DeleteMyAccount_Success_ShouldReturnNoContent()
    {
        var userId = Guid.NewGuid();
        SetUser(userId);
        _mediator.Send(Arg.Any<DeleteAccountCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(true));

        var result = await _controller.DeleteMyAccount(CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteMyAccount_NotFound_ShouldReturnNotFound()
    {
        var userId = Guid.NewGuid();
        SetUser(userId);
        _mediator.Send(Arg.Any<DeleteAccountCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<bool>(new Error("Auth.UserNotFound", "Not found")));

        var result = await _controller.DeleteMyAccount(CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // SeedAdmin

    [Fact]
    public async Task SeedAdmin_InvalidKey_ShouldReturnUnauthorized()
    {
        SetAnonymousUser();
        Environment.SetEnvironmentVariable("ADMIN_SEED_KEY", "correct-key");

        var request = new SeedAdminRequest("admin", "wrong-key");
        var result = await _controller.SeedAdmin(request, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedObjectResult>();
        Environment.SetEnvironmentVariable("ADMIN_SEED_KEY", null);
    }

    [Fact]
    public async Task SeedAdmin_UserNotFound_ShouldReturnNotFound()
    {
        SetAnonymousUser();
        Environment.SetEnvironmentVariable("ADMIN_SEED_KEY", "test-key");
        _userRepository.GetByUsernameAsync("nonexistent", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var request = new SeedAdminRequest("nonexistent", "test-key");
        var result = await _controller.SeedAdmin(request, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
        Environment.SetEnvironmentVariable("ADMIN_SEED_KEY", null);
    }

    [Fact]
    public async Task SeedAdmin_ValidRequest_ShouldPromoteAndReturnOk()
    {
        SetAnonymousUser();
        Environment.SetEnvironmentVariable("ADMIN_SEED_KEY", "test-key");
        var user = User.Create("admin", "admin@test.com", "hash", DateTime.UtcNow);
        _userRepository.GetByUsernameAsync("admin", Arg.Any<CancellationToken>())
            .Returns(user);

        var request = new SeedAdminRequest("admin", "test-key");
        var result = await _controller.SeedAdmin(request, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        user.Role.Should().Be(UserRole.Admin);
        await _userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Environment.SetEnvironmentVariable("ADMIN_SEED_KEY", null);
    }

    [Fact]
    public async Task SeedAdmin_WithEnvKey_ShouldWork()
    {
        SetAnonymousUser();
        Environment.SetEnvironmentVariable("ADMIN_SEED_KEY", "test-seed-key");
        var user = User.Create("admin", "admin@test.com", "hash", DateTime.UtcNow);
        _userRepository.GetByUsernameAsync("admin", Arg.Any<CancellationToken>())
            .Returns(user);

        var request = new SeedAdminRequest("admin", "test-seed-key");
        var result = await _controller.SeedAdmin(request, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        Environment.SetEnvironmentVariable("ADMIN_SEED_KEY", null);
    }

    [Fact]
    public async Task SeedAdmin_WithoutEnvKey_ShouldThrow()
    {
        SetAnonymousUser();
        Environment.SetEnvironmentVariable("ADMIN_SEED_KEY", null);

        var request = new SeedAdminRequest("admin", "any-key");
        var act = () => _controller.SeedAdmin(request, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // GetCurrentUserId with "sub" claim
    [Fact]
    public async Task ExportMyData_WithSubClaim_ShouldWork()
    {
        var userId = Guid.NewGuid();
        var claims = new[] { new Claim("sub", userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var response = new UserDataExportResponse(userId, "user1", "u@t.com", "User", DateTime.UtcNow, null, DateTime.UtcNow);
        _mediator.Send(Arg.Any<ExportUserDataQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(response));

        var result = await _controller.ExportMyData(CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ExportMyData_WithInvalidGuidClaim_ShouldReturnUnauthorized()
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "not-a-guid") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var result = await _controller.ExportMyData(CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }
}
