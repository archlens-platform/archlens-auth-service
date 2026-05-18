using ArchLens.Auth.Application.Contracts.Auth;
using ArchLens.Auth.Domain.Interfaces.SecurityInterfaces;
using ArchLens.Auth.Domain.Interfaces.UserInterfaces;
using ArchLens.Auth.Infrastructure;
using ArchLens.SharedKernel.Domain;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArchLens.Auth.Tests.Infrastructure;

public class InfrastructureDependencyInjectionTests
{
    private static IConfiguration BuildConfiguration(Dictionary<string, string?> settings)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    private static Dictionary<string, string?> ValidSettings => new()
    {
        ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=authdb_test;Username=test;Password=test",
        ["Jwt:Key"] = "super-secret-key-with-at-least-32-characters!!",
        ["Jwt:Issuer"] = "test-issuer",
        ["Jwt:Audience"] = "test-audience",
        ["Jwt:ExpirationMinutes"] = "60",
        ["RabbitMQ:Host"] = "localhost",
        ["RabbitMQ:Username"] = "guest",
        ["RabbitMQ:Password"] = "guest",
    };

    [Fact]
    public void AddInfrastructure_ShouldRegisterAllRequiredServices()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(ValidSettings);

        services.AddInfrastructure(configuration);

        services.Should().Contain(sd => sd.ServiceType == typeof(IUnitOfWork));
        services.Should().Contain(sd => sd.ServiceType == typeof(IUserRepository));
        services.Should().Contain(sd => sd.ServiceType == typeof(IPasswordHasher));
        services.Should().Contain(sd => sd.ServiceType == typeof(IJwtTokenGenerator));
    }

    [Fact]
    public void AddInfrastructure_ShouldThrow_WhenConnectionStringMissing()
    {
        var services = new ServiceCollection();
        var settings = ValidSettings;
        settings.Remove("ConnectionStrings:DefaultConnection");
        var configuration = BuildConfiguration(settings);

        var act = () => services.AddInfrastructure(configuration);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ConnectionStrings:DefaultConnection*");
    }

    [Fact]
    public void AddInfrastructure_ShouldThrow_WhenJwtConfigMissing()
    {
        var services = new ServiceCollection();
        var settings = ValidSettings;
        settings.Remove("Jwt:Key");
        settings.Remove("Jwt:Issuer");
        settings.Remove("Jwt:Audience");
        settings.Remove("Jwt:ExpirationMinutes");
        var configuration = BuildConfiguration(settings);

        var act = () => services.AddInfrastructure(configuration);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddMessaging_ShouldThrow_WhenRabbitMQHostMissing()
    {
        var services = new ServiceCollection();
        var settings = ValidSettings;
        settings.Remove("RabbitMQ:Host");
        settings.Remove("RabbitMQ:Username");
        settings.Remove("RabbitMQ:Password");
        var configuration = BuildConfiguration(settings);

        var act = () => services.AddInfrastructure(configuration);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*RabbitMQ*");
    }
}
