using ArchLens.Auth.Domain.Entities.UserEntities;
using ArchLens.Auth.Infrastructure.Persistence;
using ArchLens.Auth.Infrastructure.Persistence.EFCore.Context;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ArchLens.Auth.Tests.Infrastructure.Persistence;

public class UnitOfWorkTests : IDisposable
{
    private readonly AuthDbContext _context;
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AuthDbContext(options);
        _unitOfWork = new UnitOfWork(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistChanges()
    {
        var user = User.Create("uow_user", "uow@test.com", "hash", DateTime.UtcNow);
        _context.Users.Add(user);

        var result = await _unitOfWork.SaveChangesAsync();

        result.Should().BeGreaterThan(0);
        var found = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        found.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_NoChanges_ShouldReturnZero()
    {
        var result = await _unitOfWork.SaveChangesAsync();

        result.Should().Be(0);
    }

    [Fact]
    public async Task SaveChangesAsync_MultipleEntities_ShouldReturnCorrectCount()
    {
        var user1 = User.Create("user1", "user1@test.com", "hash1", DateTime.UtcNow);
        var user2 = User.Create("user2", "user2@test.com", "hash2", DateTime.UtcNow);
        _context.Users.Add(user1);
        _context.Users.Add(user2);

        var result = await _unitOfWork.SaveChangesAsync();

        result.Should().Be(2);
        var allUsers = await _context.Users.ToListAsync();
        allUsers.Should().HaveCount(2);
    }

    [Fact]
    public async Task SaveChangesAsync_WithCancellationToken_ShouldWork()
    {
        var user = User.Create("ct_user", "ct@test.com", "hash", DateTime.UtcNow);
        _context.Users.Add(user);
        using var cts = new CancellationTokenSource();

        var result = await _unitOfWork.SaveChangesAsync(cts.Token);

        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SaveChangesAsync_AfterUpdate_ShouldPersistModifications()
    {
        var user = User.Create("mod_user", "mod@test.com", "hash", DateTime.UtcNow);
        _context.Users.Add(user);
        await _unitOfWork.SaveChangesAsync();

        user.RegisterFailedLogin();
        _context.Users.Update(user);
        var result = await _unitOfWork.SaveChangesAsync();

        result.Should().BeGreaterThan(0);
        var found = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        found!.FailedLoginAttempts.Should().Be(1);
    }

    [Fact]
    public async Task SaveChangesAsync_AfterRemove_ShouldDeleteEntity()
    {
        var user = User.Create("del_user", "del@test.com", "hash", DateTime.UtcNow);
        _context.Users.Add(user);
        await _unitOfWork.SaveChangesAsync();

        _context.Users.Remove(user);
        var result = await _unitOfWork.SaveChangesAsync();

        result.Should().BeGreaterThan(0);
        var found = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        found.Should().BeNull();
    }
}

public class UnitOfWorkTransactionTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AuthDbContext _context;
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkTransactionTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AuthDbContext(options);
        _context.Database.EnsureCreated();
        _unitOfWork = new UnitOfWork(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCommitOnSuccess()
    {
        await _unitOfWork.ExecuteAsync(async ct =>
        {
            var user = User.Create("exec_user", "exec@test.com", "hash", DateTime.UtcNow);
            _context.Users.Add(user);
        });

        var users = await _context.Users.ToListAsync();
        users.Should().ContainSingle(u => u.Username == "exec_user");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRollbackOnException()
    {
        var act = async () =>
        {
            await _unitOfWork.ExecuteAsync(async ct =>
            {
                var user = User.Create("fail_user", "fail@test.com", "hash", DateTime.UtcNow);
                _context.Users.Add(user);
                throw new InvalidOperationException("forced failure");
            });
        };

        await act.Should().ThrowAsync<InvalidOperationException>();

        _context.ChangeTracker.Clear();
        var users = await _context.Users.ToListAsync();
        users.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_Generic_ShouldReturnResultAndCommit()
    {
        var result = await _unitOfWork.ExecuteAsync<string>(async ct =>
        {
            var user = User.Create("gen_user", "gen@test.com", "hash", DateTime.UtcNow);
            _context.Users.Add(user);
            return user.Username;
        });

        result.Should().Be("gen_user");

        var users = await _context.Users.ToListAsync();
        users.Should().ContainSingle(u => u.Username == "gen_user");
    }

    [Fact]
    public async Task ExecuteAsync_Generic_ShouldRollbackOnException()
    {
        var act = async () =>
        {
            await _unitOfWork.ExecuteAsync<string>(async ct =>
            {
                var user = User.Create("genfail_user", "genfail@test.com", "hash", DateTime.UtcNow);
                _context.Users.Add(user);
                throw new InvalidOperationException("forced failure");
            });
        };

        await act.Should().ThrowAsync<InvalidOperationException>();

        _context.ChangeTracker.Clear();
        var users = await _context.Users.ToListAsync();
        users.Should().BeEmpty();
    }
}
