using ArchLens.Auth.Application.Contracts.Auth;
using ArchLens.Auth.Domain.Interfaces.UserInterfaces;
using ArchLens.Auth.Domain.Interfaces.SecurityInterfaces;
using ArchLens.Auth.Infrastructure.Auth;
using ArchLens.Auth.Infrastructure.Persistence;
using ArchLens.Auth.Infrastructure.Persistence.EFCore.Context;
using ArchLens.Auth.Infrastructure.Persistence.EFCore.Repositories.UserRepositories;
using ArchLens.SharedKernel.Domain;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ArchLens.Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Configuration 'ConnectionStrings:DefaultConnection' is required");

        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(connectionString));

        var jwtOptions = configuration.GetRequiredSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Configuration 'Jwt' is required");

        services.Configure<JwtOptions>(configuration.GetRequiredSection(JwtOptions.SectionName));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                    ValidateLifetime = true,
                };
            });

        services.AddAuthorization();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddMessaging(configuration);

        return services;
    }

    private static void AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitSection = configuration.GetRequiredSection("RabbitMQ");
        var host = rabbitSection["Host"] ?? throw new InvalidOperationException("Configuration 'RabbitMQ:Host' is required");
        var username = rabbitSection["Username"] ?? throw new InvalidOperationException("Configuration 'RabbitMQ:Username' is required");
        var password = rabbitSection["Password"] ?? throw new InvalidOperationException("Configuration 'RabbitMQ:Password' is required");

        services.AddMassTransit(bus =>
        {
            bus.SetKebabCaseEndpointNameFormatter();

            bus.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(host, "/", h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });
    }
}
