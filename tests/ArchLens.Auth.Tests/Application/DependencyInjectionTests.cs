using ArchLens.Auth.Application;
using ArchLens.Auth.Application.Behaviors;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ArchLens.Auth.Tests.Application;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_ShouldRegisterMediatR()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        services.Should().Contain(sd => sd.ServiceType == typeof(IMediator));
    }

    [Fact]
    public void AddApplication_ShouldRegisterValidationBehavior()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        services.Should().Contain(sd =>
            sd.ServiceType == typeof(IPipelineBehavior<,>) &&
            sd.ImplementationType == typeof(ValidationBehavior<,>));
    }

    [Fact]
    public void AddApplication_ShouldReturnSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddApplication();

        result.Should().BeSameAs(services);
    }
}
