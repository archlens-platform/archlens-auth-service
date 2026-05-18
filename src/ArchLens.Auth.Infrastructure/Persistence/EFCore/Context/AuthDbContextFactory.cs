using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ArchLens.Auth.Infrastructure.Persistence.EFCore.Context;

internal sealed class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("DOTNET_CONNECTION_STRING")
            ?? "Host=localhost;Database=archlens_auth;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AuthDbContext(options);
    }
}
