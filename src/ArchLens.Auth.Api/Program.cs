using ArchLens.Auth.Api.Configurations;
using ArchLens.Auth.Api.ExceptionHandlers;
using ArchLens.Auth.Api.Middlewares;
using ArchLens.Auth.Application;
using ArchLens.Auth.Domain.Entities.UserEntities;
using ArchLens.Auth.Domain.Interfaces.SecurityInterfaces;
using ArchLens.Auth.Domain.Interfaces.UserInterfaces;
using ArchLens.Auth.Infrastructure;
using ArchLens.Auth.Infrastructure.Persistence.EFCore.Context;
using Microsoft.EntityFrameworkCore;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddSerilogLogging();
    builder.AddOpenTelemetryObservability();
    builder.AddRateLimiting();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Informe o token JWT: Bearer {token}"
        });
        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                []
            }
        });
    });
    builder.Services.AddHealthChecks()
        .AddNpgSql(
            builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection configuration is required."),
            name: "postgresql",
            tags: ["db"]);
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddCors(options =>
        options.AddDefaultPolicy(policy =>
        {
            var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? ["http://localhost:3000"];
            policy.WithOrigins(origins).AllowAnyMethod().AllowAnyHeader();
        }));

    var app = builder.Build();

    if (!app.Environment.IsEnvironment("Testing"))
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await db.Database.MigrateAsync();

        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var defaultUserPassword = Environment.GetEnvironmentVariable("DEFAULT_USER_PASSWORD") ?? builder.Configuration["Seed:DefaultUserPassword"]
            ?? throw new InvalidOperationException("Seed:DefaultUserPassword configuration or DEFAULT_USER_PASSWORD env var is required.");
        var defaultAdminPassword = Environment.GetEnvironmentVariable("DEFAULT_ADMIN_PASSWORD") ?? builder.Configuration["Seed:DefaultAdminPassword"]
            ?? throw new InvalidOperationException("Seed:DefaultAdminPassword configuration or DEFAULT_ADMIN_PASSWORD env var is required.");

        if (await userRepo.GetByUsernameAsync("user", default) is null)
        {
            var normalUser = User.Create("user", "user@archlens.com", hasher.Hash(defaultUserPassword), DateTime.UtcNow);
            await userRepo.AddAsync(normalUser, default);
            Log.Information("Seeded default user: user");
        }

        if (await userRepo.GetByUsernameAsync("admin", default) is null)
        {
            var adminUser = User.Create("admin", "admin@archlens.com", hasher.Hash(defaultAdminPassword), DateTime.UtcNow, UserRole.Admin);
            await userRepo.AddAsync(adminUser, default);
            Log.Information("Seeded default admin: admin");
        }
        else
        {
            var existingAdmin = await userRepo.GetByUsernameAsync("admin", default);
            if (existingAdmin is not null && existingAdmin.Role != UserRole.Admin)
            {
                existingAdmin.PromoteToAdmin();
                await userRepo.UpdateAsync(existingAdmin, default);
                Log.Information("Promoted existing user 'admin' to Admin role");
            }
        }
    }

    app.UseExceptionHandler();
    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<PersonalDataAccessAuditMiddleware>();
    app.UseSerilogRequestLogging();
    if (!app.Environment.IsEnvironment("Testing"))
        app.UseRateLimiter();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();

    if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.MapControllers();
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

namespace ArchLens.Auth.Api
{
    public partial class Program;
}
