using ArchLens.Auth.Api.Middlewares;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace ArchLens.Auth.Tests.Api.Middlewares;

public class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_NoHeader_ShouldGenerateCorrelationId()
    {
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new CorrelationIdMiddleware(next);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
        context.Response.Headers["X-Correlation-Id"].ToString().Should().NotBeNullOrWhiteSpace();
        context.Items["X-Correlation-Id"].Should().NotBeNull();
    }

    [Fact]
    public async Task InvokeAsync_WithHeader_ShouldUseExistingCorrelationId()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new CorrelationIdMiddleware(next);
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-Id"] = "my-correlation-id";

        await middleware.InvokeAsync(context);

        context.Response.Headers["X-Correlation-Id"].ToString().Should().Be("my-correlation-id");
        context.Items["X-Correlation-Id"]!.ToString().Should().Be("my-correlation-id");
    }

    [Fact]
    public async Task InvokeAsync_EmptyHeader_ShouldGenerateNewId()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new CorrelationIdMiddleware(next);
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-Id"] = "";

        await middleware.InvokeAsync(context);

        context.Response.Headers["X-Correlation-Id"].ToString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNext()
    {
        var callOrder = new List<string>();
        RequestDelegate next = _ => { callOrder.Add("next"); return Task.CompletedTask; };
        var middleware = new CorrelationIdMiddleware(next);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        callOrder.Should().Contain("next");
    }
}
