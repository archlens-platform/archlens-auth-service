using System.Security.Claims;
using System.Text.Encodings.Web;
using ArchLens.Auth.Infrastructure.Persistence.EFCore.Context;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reqnroll;

namespace ArchLens.Auth.Tests.BDD.Hooks;

[Binding]
public sealed class TestHooks
{
    private static BddWebApplicationFactory _factory = null!;
    private static HttpClient _client = null!;

    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection",
            "Host=localhost;Database=auth_bdd_test;Username=test;Password=test");
        Environment.SetEnvironmentVariable("Jwt__Key",
            "BddTestKeyThatIsAtLeast32CharactersLongForSecurity!!");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "archlens-bdd-test");
        Environment.SetEnvironmentVariable("Jwt__Audience", "archlens-bdd-test");
        Environment.SetEnvironmentVariable("Jwt__ExpirationMinutes", "60");
        Environment.SetEnvironmentVariable("RabbitMQ__Host", "localhost");
        Environment.SetEnvironmentVariable("RabbitMQ__Username", "guest");
        Environment.SetEnvironmentVariable("RabbitMQ__Password", "guest");

        _factory = new BddWebApplicationFactory();
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        db.Database.EnsureCreated();
    }

    [AfterTestRun]
    public static void AfterTestRun()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [BeforeScenario]
    public void BeforeScenario(ScenarioContext scenarioContext)
    {
        BddTestAuthHandler.Reset();

        // Clean InMemory DB before each scenario
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            db.Users.RemoveRange(db.Users);
            db.SaveChanges();
            db.ChangeTracker.Clear();
        }

        scenarioContext.Set(_client, "HttpClient");
        scenarioContext.Set(_factory, "Factory");
    }
}

public sealed class BddWebApplicationFactory : WebApplicationFactory<ArchLens.Auth.Api.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove ALL DbContext and EF provider registrations
            var descriptorsToRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<AuthDbContext>)
                    || d.ServiceType == typeof(DbContextOptions)
                    || (d.ServiceType.IsGenericType &&
                        d.ServiceType.GetGenericTypeDefinition().FullName?.Contains("EntityFrameworkCore") == true)
                    || d.ServiceType.FullName?.Contains("Npgsql") == true
                    || d.ServiceType.FullName?.Contains("EntityFrameworkCore.Relational") == true
                    || d.ImplementationType?.FullName?.Contains("Npgsql") == true)
                .ToList();
            foreach (var descriptor in descriptorsToRemove)
                services.Remove(descriptor);

            // Add InMemory DB
            services.AddDbContext<AuthDbContext>(options =>
                options.UseInMemoryDatabase("AuthBddTests"));

            // Replace MassTransit with TestHarness
            var massTransitDescriptors = services
                .Where(d => d.ServiceType.FullName?.Contains("MassTransit") == true)
                .ToList();
            foreach (var descriptor in massTransitDescriptors)
                services.Remove(descriptor);

            services.AddMassTransitTestHarness();

            // Remove HostedServices
            var hostedServices = services
                .Where(d => d.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService))
                .ToList();
            foreach (var descriptor in hostedServices)
                services.Remove(descriptor);

            // Add test auth handler and force it as the default scheme
            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, BddTestAuthHandler>("Test", _ => { });

            services.PostConfigureAll<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
                options.DefaultScheme = "Test";
            });
        });
    }
}

public sealed class BddTestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private static bool _isAuthenticated;
    private static string _userId = Guid.NewGuid().ToString();
    private static string _role = "User";

    public BddTestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    public static void SetAuthenticated(string userId, string role = "User")
    {
        _isAuthenticated = true;
        _userId = userId;
        _role = role;
    }

    public static void Reset()
    {
        _isAuthenticated = false;
        _userId = Guid.NewGuid().ToString();
        _role = "User";
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!_isAuthenticated)
            return Task.FromResult(AuthenticateResult.Fail("Not authenticated"));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _userId),
            new Claim("sub", _userId),
            new Claim(ClaimTypes.Role, _role),
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
