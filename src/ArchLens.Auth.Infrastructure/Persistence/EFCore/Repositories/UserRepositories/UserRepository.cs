using ArchLens.Auth.Domain.Entities.UserEntities;
using ArchLens.Auth.Domain.Interfaces.UserInterfaces;
using ArchLens.Auth.Infrastructure.Persistence.EFCore.Context;
using Microsoft.EntityFrameworkCore;

namespace ArchLens.Auth.Infrastructure.Persistence.EFCore.Repositories.UserRepositories;

public sealed class UserRepository(AuthDbContext db) : IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        await db.Users.FirstOrDefaultAsync(u => u.Username == username, ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<bool> ExistsByUsernameOrEmailAsync(string username, string email, CancellationToken ct = default) =>
        await db.Users.AnyAsync(u => u.Username == username || u.Email == email, ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await db.Users.AddAsync(user, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        db.Users.Update(user);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(User user, CancellationToken ct = default)
    {
        db.Users.Remove(user);
        await db.SaveChangesAsync(ct);
    }
}
