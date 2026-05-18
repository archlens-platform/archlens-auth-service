using ArchLens.Auth.Domain.Entities.UserEntities;
using ArchLens.Auth.Infrastructure.Persistence.EFCore.Context;
using ArchLens.Auth.Infrastructure.Persistence.EFCore.Repositories.UserRepositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ArchLens.Auth.Tests.Infrastructure.Persistence;

public class UserRepositoryTests : IDisposable
{
    private readonly AuthDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AuthDbContext(options);
        _repository = new UserRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private static User CreateUser(string username = "testuser", string email = "test@test.com") =>
        User.Create(username, email, "hashedpw", DateTime.UtcNow);

    [Fact]
    public async Task AddAsync_ShouldPersistUser()
    {
        var user = CreateUser();

        await _repository.AddAsync(user);

        var found = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        found.Should().NotBeNull();
        found!.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ShouldReturnUser()
    {
        var user = CreateUser();
        await _repository.AddAsync(user);

        var result = await _repository.GetByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingUser_ShouldReturnNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_ExistingUser_ShouldReturnUser()
    {
        var user = CreateUser();
        await _repository.AddAsync(user);

        var result = await _repository.GetByUsernameAsync("testuser");

        result.Should().NotBeNull();
        result!.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task GetByUsernameAsync_NonExisting_ShouldReturnNull()
    {
        var result = await _repository.GetByUsernameAsync("nobody");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_ExistingUser_ShouldReturnUser()
    {
        var user = CreateUser();
        await _repository.AddAsync(user);

        var result = await _repository.GetByEmailAsync("test@test.com");

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_NonExisting_ShouldReturnNull()
    {
        var result = await _repository.GetByEmailAsync("nobody@test.com");

        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsByUsernameOrEmailAsync_MatchingUsername_ShouldReturnTrue()
    {
        var user = CreateUser();
        await _repository.AddAsync(user);

        var result = await _repository.ExistsByUsernameOrEmailAsync("testuser", "other@test.com");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByUsernameOrEmailAsync_MatchingEmail_ShouldReturnTrue()
    {
        var user = CreateUser();
        await _repository.AddAsync(user);

        var result = await _repository.ExistsByUsernameOrEmailAsync("other", "test@test.com");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByUsernameOrEmailAsync_NoMatch_ShouldReturnFalse()
    {
        var result = await _repository.ExistsByUsernameOrEmailAsync("nobody", "nobody@test.com");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        var user = CreateUser();
        await _repository.AddAsync(user);

        user.RegisterFailedLogin();
        await _repository.UpdateAsync(user);

        var updated = await _repository.GetByIdAsync(user.Id);
        updated!.FailedLoginAttempts.Should().Be(1);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveUser()
    {
        var user = CreateUser();
        await _repository.AddAsync(user);

        await _repository.DeleteAsync(user);

        var result = await _repository.GetByIdAsync(user.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistAllUserProperties()
    {
        var consentDate = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var user = User.Create("fulluser", "full@test.com", "securehash", consentDate, UserRole.Admin);

        await _repository.AddAsync(user);

        var found = await _repository.GetByIdAsync(user.Id);
        found.Should().NotBeNull();
        found!.Username.Should().Be("fulluser");
        found.Email.Should().Be("full@test.com");
        found.PasswordHash.Should().Be("securehash");
        found.Role.Should().Be(UserRole.Admin);
        found.IsActive.Should().BeTrue();
        found.FailedLoginAttempts.Should().Be(0);
        found.LockoutEnd.Should().BeNull();
        found.LgpdConsentGivenAt.Should().Be(consentDate);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnCorrectUser_WhenMultipleUsersExist()
    {
        var user1 = CreateUser("user1", "user1@test.com");
        var user2 = CreateUser("user2", "user2@test.com");
        var user3 = CreateUser("user3", "user3@test.com");
        await _repository.AddAsync(user1);
        await _repository.AddAsync(user2);
        await _repository.AddAsync(user3);

        var result = await _repository.GetByEmailAsync("user2@test.com");

        result.Should().NotBeNull();
        result!.Id.Should().Be(user2.Id);
        result.Username.Should().Be("user2");
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnCorrectUser_WhenMultipleUsersExist()
    {
        var user1 = CreateUser("alice", "alice@test.com");
        var user2 = CreateUser("bob", "bob@test.com");
        await _repository.AddAsync(user1);
        await _repository.AddAsync(user2);

        var result = await _repository.GetByUsernameAsync("bob");

        result.Should().NotBeNull();
        result!.Id.Should().Be(user2.Id);
        result.Email.Should().Be("bob@test.com");
    }

    [Fact]
    public async Task ExistsByUsernameOrEmailAsync_BothMatch_ShouldReturnTrue()
    {
        var user = CreateUser();
        await _repository.AddAsync(user);

        var result = await _repository.ExistsByUsernameOrEmailAsync("testuser", "test@test.com");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldNotAffectOtherUsers()
    {
        var user1 = CreateUser("keep", "keep@test.com");
        var user2 = CreateUser("remove", "remove@test.com");
        await _repository.AddAsync(user1);
        await _repository.AddAsync(user2);

        await _repository.DeleteAsync(user2);

        var remaining = await _repository.GetByIdAsync(user1.Id);
        remaining.Should().NotBeNull();
        remaining!.Username.Should().Be("keep");

        var deleted = await _repository.GetByIdAsync(user2.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistSuccessfulLogin()
    {
        var user = CreateUser();
        await _repository.AddAsync(user);

        user.RegisterSuccessfulLogin();
        await _repository.UpdateAsync(user);

        var updated = await _repository.GetByIdAsync(user.Id);
        updated.Should().NotBeNull();
        updated!.LastLoginAt.Should().NotBeNull();
        updated.FailedLoginAttempts.Should().Be(0);
        updated.LockoutEnd.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistLockout()
    {
        var user = CreateUser();
        await _repository.AddAsync(user);

        for (int i = 0; i < User.MaxFailedAttempts; i++)
            user.RegisterFailedLogin();
        await _repository.UpdateAsync(user);

        var updated = await _repository.GetByIdAsync(user.Id);
        updated.Should().NotBeNull();
        updated!.FailedLoginAttempts.Should().Be(User.MaxFailedAttempts);
        updated.LockoutEnd.Should().NotBeNull();
    }

    [Fact]
    public async Task AddAsync_WithCancellationToken_ShouldWork()
    {
        var user = CreateUser();
        using var cts = new CancellationTokenSource();

        await _repository.AddAsync(user, cts.Token);

        var found = await _repository.GetByIdAsync(user.Id);
        found.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithCancellationToken_ShouldWork()
    {
        var user = CreateUser();
        await _repository.AddAsync(user);
        using var cts = new CancellationTokenSource();

        var result = await _repository.GetByIdAsync(user.Id, cts.Token);

        result.Should().NotBeNull();
    }
}
