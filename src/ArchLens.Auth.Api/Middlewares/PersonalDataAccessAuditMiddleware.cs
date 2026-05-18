using System.Security.Claims;

namespace ArchLens.Auth.Api.Middlewares;

public sealed class PersonalDataAccessAuditMiddleware(RequestDelegate next, ILogger<PersonalDataAccessAuditMiddleware> logger)
{
    private static readonly HashSet<string> AuditedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/auth/me/data",
        "/auth/me",
    };

    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);

        if (ShouldAudit(context))
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? context.User.FindFirstValue("sub");

            logger.LogInformation(
                "PERSONAL_DATA_ACCESS | Method: {Method} | Path: {Path} | UserId: {UserId} | Status: {Status} | IP: {IP} | TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path,
                userId ?? "anonymous",
                context.Response.StatusCode,
                context.Connection.RemoteIpAddress,
                context.TraceIdentifier);
        }
    }

    private static bool ShouldAudit(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        return AuditedPaths.Any(p => path.EndsWith(p, StringComparison.OrdinalIgnoreCase));
    }
}
