using ArchLens.SharedKernel.Domain;
using ArchLens.Auth.Infrastructure.Persistence.EFCore.Context;
using Microsoft.EntityFrameworkCore;

namespace ArchLens.Auth.Infrastructure.Persistence;

public sealed class UnitOfWork(AuthDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);

    public async Task ExecuteAsync(Func<CancellationToken, Task> work, CancellationToken ct = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await context.Database.BeginTransactionAsync(ct);
            try
            {
                await work(ct);
                await context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        });
    }

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken ct = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await context.Database.BeginTransactionAsync(ct);
            try
            {
                var result = await work(ct);
                await context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return result;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        });
    }
}
