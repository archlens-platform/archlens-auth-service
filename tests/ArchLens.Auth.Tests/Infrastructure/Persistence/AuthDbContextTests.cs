using ArchLens.Auth.Domain.Entities.UserEntities;
using ArchLens.Auth.Infrastructure.Persistence.EFCore.Context;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ArchLens.Auth.Tests.Infrastructure.Persistence;

public class AuthDbContextTests : IDisposable
{
    private readonly AuthDbContext _context;

    public AuthDbContextTests()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AuthDbContext(options);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public void Users_DbSet_ShouldNotBeNull()
    {
        _context.Users.Should().NotBeNull();
    }

    [Fact]
    public async Task CanAddAndRetrieveUser()
    {
        var user = User.Create("dbtest", "db@test.com", "hash", DateTime.UtcNow);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var retrieved = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Username.Should().Be("dbtest");
    }

    [Fact]
    public async Task ShouldApplyEntityConfiguration_UserTableMapping()
    {
        var entityType = _context.Model.FindEntityType(typeof(User));

        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("users");
    }

    [Fact]
    public void ShouldApplyEntityConfiguration_UserProperties()
    {
        var entityType = _context.Model.FindEntityType(typeof(User))!;

        var username = entityType.FindProperty(nameof(User.Username))!;
        username.GetMaxLength().Should().Be(50);
        username.IsNullable.Should().BeFalse();

        var email = entityType.FindProperty(nameof(User.Email))!;
        email.GetMaxLength().Should().Be(256);
        email.IsNullable.Should().BeFalse();

        var passwordHash = entityType.FindProperty(nameof(User.PasswordHash))!;
        passwordHash.GetMaxLength().Should().Be(256);
        passwordHash.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void ShouldApplyEntityConfiguration_UniqueIndexes()
    {
        var entityType = _context.Model.FindEntityType(typeof(User))!;

        var usernameIndex = entityType.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(User.Username)));
        usernameIndex.Should().NotBeNull();
        usernameIndex!.IsUnique.Should().BeTrue();

        var emailIndex = entityType.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(User.Email)));
        emailIndex.Should().NotBeNull();
        emailIndex!.IsUnique.Should().BeTrue();
    }

    [Fact]
    public void ShouldApplyEntityConfiguration_PrimaryKey()
    {
        var entityType = _context.Model.FindEntityType(typeof(User))!;

        var primaryKey = entityType.FindPrimaryKey();
        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().ContainSingle(p => p.Name == nameof(User.Id));
    }

    [Fact]
    public void ShouldApplyEntityConfiguration_LgpdConsentGivenAtIsOptional()
    {
        var entityType = _context.Model.FindEntityType(typeof(User))!;

        var lgpdProperty = entityType.FindProperty(nameof(User.LgpdConsentGivenAt))!;
        lgpdProperty.IsNullable.Should().BeTrue();
    }

    [Fact]
    public void ShouldApplyEntityConfiguration_RoleIsStoredAsString()
    {
        var entityType = _context.Model.FindEntityType(typeof(User))!;

        var roleProperty = entityType.FindProperty(nameof(User.Role))!;
        roleProperty.GetMaxLength().Should().Be(20);
    }

    [Fact]
    public async Task ShouldPersistMultipleUsersIndependently()
    {
        var user1 = User.Create("user1", "user1@test.com", "hash1", DateTime.UtcNow);
        var user2 = User.Create("user2", "user2@test.com", "hash2", DateTime.UtcNow);
        _context.Users.Add(user1);
        _context.Users.Add(user2);
        await _context.SaveChangesAsync();

        var count = await _context.Users.CountAsync();
        count.Should().Be(2);
    }
}
