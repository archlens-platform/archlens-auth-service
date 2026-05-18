using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.OpenTelemetry;

namespace ArchLens.Auth.Api.Configurations;

public static class SerilogExtensions
{
    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder, string serviceName = "auth-service")
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        builder.Host.UseSerilog((context, configuration) =>
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ServiceName", serviceName)
                .Enrich.WithProperty("Application", "archlens")
                .Enrich.With<EmailMaskingEnricher>()
                .WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = context.Configuration["Otlp:Endpoint"]
                        ?? "http://otel-collector:4317";
                    options.Protocol = OtlpProtocol.Grpc;
                    options.ResourceAttributes = new Dictionary<string, object>
                    {
                        ["service.name"] = serviceName
                    };
                }));

        return builder;
    }
}

internal sealed class EmailMaskingEnricher : ILogEventEnricher
{
    private static readonly System.Text.RegularExpressions.Regex EmailRegex =
        new(@"[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}",
            System.Text.RegularExpressions.RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var newProperties = new Dictionary<string, LogEventPropertyValue>();

        foreach (var property in logEvent.Properties)
        {
            if (property.Value is ScalarValue sv && sv.Value is string str && EmailRegex.IsMatch(str))
            {
                newProperties[property.Key] = new ScalarValue(EmailRegex.Replace(str, MaskEmail));
            }
        }

        foreach (var (key, value) in newProperties)
            logEvent.AddPropertyIfAbsent(new LogEventProperty(key, value));
    }

    private static string MaskEmail(System.Text.RegularExpressions.Match m)
    {
        var parts = m.Value.Split('@');
        if (parts.Length < 2) return "***@***";
        var local = parts[0];
        var masked = local.Length > 2
            ? $"{local[0]}***{local[^1]}"
            : "***";
        return $"{masked}@{parts[1]}";
    }
}
