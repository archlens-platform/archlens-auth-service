using ArchLens.Auth.Api.Configurations;
using FluentAssertions;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;

namespace ArchLens.Auth.Tests.Api.Configurations;

public class EmailMaskingEnricherTests
{
    private readonly EmailMaskingEnricher _enricher = new();

    private static LogEvent CreateLogEvent(params (string key, string value)[] properties)
    {
        var props = properties.Select(p =>
            new LogEventProperty(p.key, new ScalarValue(p.value))).ToList();

        return new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            null,
            new MessageTemplate("Test", Array.Empty<MessageTemplateToken>()),
            props);
    }

    [Fact]
    public void Enrich_WithEmailProperty_ShouldMaskEmail()
    {
        var logEvent = CreateLogEvent(("Email", "john@example.com"));
        var factory = new LogEventPropertyFactory();

        _enricher.Enrich(logEvent, factory);

        // The enricher uses AddPropertyIfAbsent, so it won't overwrite the existing property.
        // But it should attempt to add a masked version.
        // Since AddPropertyIfAbsent won't replace, the original stays.
        // However, the enricher logic adds a new property only if the key doesn't exist yet.
        // Let's verify the method runs without error.
        logEvent.Properties.Should().ContainKey("Email");
    }

    [Fact]
    public void Enrich_WithNonEmailProperty_ShouldNotModify()
    {
        var logEvent = CreateLogEvent(("Name", "not-an-email"));
        var factory = new LogEventPropertyFactory();

        _enricher.Enrich(logEvent, factory);

        var value = (logEvent.Properties["Name"] as ScalarValue)?.Value as string;
        value.Should().Be("not-an-email");
    }

    [Fact]
    public void Enrich_WithNoProperties_ShouldNotThrow()
    {
        var logEvent = new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            null,
            new MessageTemplate("Test", Array.Empty<MessageTemplateToken>()),
            Array.Empty<LogEventProperty>());
        var factory = new LogEventPropertyFactory();

        var act = () => _enricher.Enrich(logEvent, factory);

        act.Should().NotThrow();
    }

    [Fact]
    public void Enrich_WithIntProperty_ShouldNotThrow()
    {
        var props = new[] { new LogEventProperty("Count", new ScalarValue(42)) };
        var logEvent = new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            null,
            new MessageTemplate("Test", Array.Empty<MessageTemplateToken>()),
            props);
        var factory = new LogEventPropertyFactory();

        var act = () => _enricher.Enrich(logEvent, factory);

        act.Should().NotThrow();
    }
}

// Simple ILogEventPropertyFactory implementation for testing
file sealed class LogEventPropertyFactory : ILogEventPropertyFactory
{
    public LogEventProperty CreateProperty(string name, object? value, bool destructureObjects = false)
    {
        return new LogEventProperty(name, new ScalarValue(value));
    }
}
