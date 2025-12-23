using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace KinoAppDB.Repository
{
    /// <summary>
    /// Base EF Core repository implementation providing common CRUD operations for entities.
    /// </summary>
    /// <typeparam name="TEntity">Entity type managed by this repository.</typeparam>
    /// <remarks>
    /// The repository relies on <see cref="KinoAppDbContextScope"/> to supply the current <see cref="KinoAppDbContext"/>
    /// instance. Query methods default to <c>AsNoTracking</c> to keep read operations lightweight.
    /// </remarks>
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly KinoAppDbContextScope _scope;

        /// <summary>
        /// Gets the current EF Core database context for this scope.
        /// </summary>
        protected KinoAppDbContext Ctx => _scope.Current;

        /// <summary>
        /// Gets the <see cref="DbSet{TEntity}"/> for the entity type.
        /// </summary>
        protected DbSet<TEntity> Set => Ctx.Set<TEntity>();

        /// <summary>
        /// Creates a new <see cref="Repository{TEntity}"/>.
        /// </summary>
        /// <param name="scope">Context scope providing the active <see cref="KinoAppDbContext"/>.</param>
        public Repository(KinoAppDbContextScope scope) => _scope = scope;

        /// <inheritdoc />
        public Task<TEntity?> GetByIdAsync(object id, CancellationToken ct = default)
            => Set.FindAsync(new[] { id }, ct).AsTask();

        /// <inheritdoc />
        public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default)
            => await Set.AsNoTracking().ToListAsync(ct);

        /// <inheritdoc />
        public IQueryable<TEntity> Query(bool asNoTracking = true)
            => asNoTracking ? Set.AsNoTracking() : Set.AsQueryable();

        /// <inheritdoc />
        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
            => Set.AsNoTracking().AnyAsync(predicate, ct);

        /// <inheritdoc />
        public Task AddAsync(TEntity entity, CancellationToken ct = default)
            => Set.AddAsync(entity, ct).AsTask();

        /// <inheritdoc />
        public Task UpdateAsync(TEntity entity, CancellationToken ct = default)
        {
            Set.Update(entity);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task DeleteAsync(TEntity entity, CancellationToken ct = default)
        {
            Set.Remove(entity);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SaveAsync(CancellationToken ct = default)
            => Ctx.SaveChangesAsync(ct);
    }
}
