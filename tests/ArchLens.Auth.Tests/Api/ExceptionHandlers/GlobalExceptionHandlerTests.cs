using System.Net;
using System.Text.Json;
using ArchLens.Auth.Api.ExceptionHandlers;
using ArchLens.SharedKernel.Domain;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ArchLens.Auth.Tests.Api.ExceptionHandlers;

// Concrete DomainException for testing
file sealed class TestDomainException : DomainException
{
    public TestDomainException(string code, string message) : base(code, message) { }
}

public class GlobalExceptionHandlerTests
{
    private readonly ILogger<GlobalExceptionHandler> _logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        _handler = new GlobalExceptionHandler(_logger);
    }

    private static HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    [Fact]
    public async Task TryHandleAsync_ValidationException_ShouldReturn400()
    {
        var context = CreateHttpContext();
        var failures = new[] { new ValidationFailure("Name", "Required") };
        var exception = new ValidationException(failures);

        var handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task TryHandleAsync_ValidationException_ShouldWriteProblemDetails()
    {
        var context = CreateHttpContext();
        var failures = new[] { new ValidationFailure("Email", "Invalid") };
        var exception = new ValidationException(failures);

        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        context.Response.Body.Position = 0;
        var json = await JsonDocument.ParseAsync(context.Response.Body);
        json.RootElement.GetProperty("title").GetString().Should().Be("Validation Error");
        json.RootElement.GetProperty("status").GetInt32().Should().Be(400);
    }

    [Fact]
    public async Task TryHandleAsync_DomainException_ShouldReturn422()
    {
        var context = CreateHttpContext();
        var exception = new TestDomainException("Domain.Error", "Something went wrong in the domain");

        var handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task TryHandleAsync_DomainException_ShouldWriteProblemDetailsWithCode()
    {
        var context = CreateHttpContext();
        var exception = new TestDomainException("TestCode", "Test message");

        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        context.Response.Body.Position = 0;
        var json = await JsonDocument.ParseAsync(context.Response.Body);
        json.RootElement.GetProperty("title").GetString().Should().Be("Domain Error");
        json.RootElement.GetProperty("detail").GetString().Should().Be("Test message");
    }

    [Fact]
    public async Task TryHandleAsync_UnhandledException_ShouldReturn500()
    {
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Unexpected");

        var handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task TryHandleAsync_UnhandledException_ShouldLogError()
    {
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("DB down");

        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task TryHandleAsync_UnhandledException_ShouldNotExposeDetails()
    {
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Sensitive internal error");

        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        context.Response.Body.Position = 0;
        var json = await JsonDocument.ParseAsync(context.Response.Body);
        json.RootElement.GetProperty("detail").GetString().Should().Be("An unexpected error occurred.");
    }
}
