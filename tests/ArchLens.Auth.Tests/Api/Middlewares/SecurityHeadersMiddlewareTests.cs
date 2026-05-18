using ArchLens.Auth.Api.Middlewares;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace ArchLens.Auth.Tests.Api.Middlewares;

public class SecurityHeadersMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ShouldSetXContentTypeOptions()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.Headers["X-Content-Type-Options"].ToString().Should().Be("nosniff");
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetXFrameOptions()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.Headers["X-Frame-Options"].ToString().Should().Be("DENY");
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetXXssProtection()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.Headers["X-XSS-Protection"].ToString().Should().Be("0");
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetReferrerPolicy()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.Headers["Referrer-Policy"].ToString().Should().Be("strict-origin-when-cross-origin");
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetPermissionsPolicy()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.Headers["Permissions-Policy"].ToString().Should().Be("camera=(), microphone=(), geolocation=()");
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetCacheControl()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.Headers["Cache-Control"].ToString().Should().Be("no-store");
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNext()
    {
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new SecurityHeadersMiddleware(next);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }
}
