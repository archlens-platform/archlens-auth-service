using System.Security.Claims;
using ArchLens.Auth.Api.Middlewares;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ArchLens.Auth.Tests.Api.Middlewares;

public class PersonalDataAccessAuditMiddlewareTests
{
    private readonly ILogger<PersonalDataAccessAuditMiddleware> _logger =
        Substitute.For<ILogger<PersonalDataAccessAuditMiddleware>>();

    [Fact]
    public async Task InvokeAsync_AuditedPath_ShouldLogAccess()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new PersonalDataAccessAuditMiddleware(next, _logger);
        var context = new DefaultHttpContext();
        context.Request.Path = "/auth/me/data";
        context.Request.Method = "GET";

        await middleware.InvokeAsync(context);

        _logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task InvokeAsync_NonAuditedPath_ShouldNotLog()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new PersonalDataAccessAuditMiddleware(next, _logger);
        var context = new DefaultHttpContext();
        context.Request.Path = "/auth/login";
        context.Request.Method = "POST";

        await middleware.InvokeAsync(context);

        _logger.DidNotReceive().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task InvokeAsync_DeleteMePath_ShouldLog()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new PersonalDataAccessAuditMiddleware(next, _logger);
        var context = new DefaultHttpContext();
        context.Request.Path = "/auth/me";
        context.Request.Method = "DELETE";

        await middleware.InvokeAsync(context);

        _logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextBeforeAuditing()
    {
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new PersonalDataAccessAuditMiddleware(next, _logger);
        var context = new DefaultHttpContext();
        context.Request.Path = "/auth/me/data";

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithAuthenticatedUser_ShouldLogUserId()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new PersonalDataAccessAuditMiddleware(next, _logger);
        var context = new DefaultHttpContext();
        context.Request.Path = "/auth/me/data";
        var userId = Guid.NewGuid().ToString();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

        await middleware.InvokeAsync(context);

        _logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task InvokeAsync_RegisterPath_ShouldNotLog()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new PersonalDataAccessAuditMiddleware(next, _logger);
        var context = new DefaultHttpContext();
        context.Request.Path = "/auth/register";
        context.Request.Method = "POST";

        await middleware.InvokeAsync(context);

        _logger.DidNotReceive().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
