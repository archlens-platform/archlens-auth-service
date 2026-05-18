using ArchLens.Auth.Domain.Entities.UserEntities;
using Microsoft.EntityFrameworkCore;

namespace ArchLens.Auth.Infrastructure.Persistence.EFCore.Context;

public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
    }
}
