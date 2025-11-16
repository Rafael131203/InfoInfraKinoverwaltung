using KinoAppCore.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace KinoAppDB.Repository;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly KinoAppDbContextScope _scope;
    protected KinoAppDbContext Ctx => _scope.Current;
    protected DbSet<TEntity> Set => Ctx.Set<TEntity>();

    public Repository(KinoAppDbContextScope scope) => _scope = scope;

    public Task<TEntity?> GetByIdAsync(object id, CancellationToken ct = default)
        => Set.FindAsync([id], ct).AsTask();

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default)
        => await Set.AsNoTracking().ToListAsync(ct);

    public IQueryable<TEntity> Query(bool asNoTracking = true)
        => asNoTracking ? Set.AsNoTracking() : Set.AsQueryable();

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
        => Set.AsNoTracking().AnyAsync(predicate, ct);

    public Task AddAsync(TEntity entity, CancellationToken ct = default)
        => Set.AddAsync(entity, ct).AsTask();

    public Task UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        Set.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TEntity entity, CancellationToken ct = default)
    {
        Set.Remove(entity);
        return Task.CompletedTask;
    }

    public Task SaveAsync(CancellationToken ct = default) => Ctx.SaveChangesAsync(ct);
}
