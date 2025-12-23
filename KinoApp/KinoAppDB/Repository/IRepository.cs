using System.Linq.Expressions;

namespace KinoAppDB.Repository
{
    /// <summary>
    /// Generic repository abstraction for CRUD access to EF Core entities.
    /// </summary>
    /// <typeparam name="TEntity">Entity type managed by the repository.</typeparam>
    /// <remarks>
    /// Implementations typically operate on an EF Core <c>DbContext</c> provided by a scope/lifetime wrapper and
    /// expose an <see cref="IQueryable{T}"/> for composing queries.
    /// </remarks>
    public interface IRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Retrieves an entity by its primary key.
        /// </summary>
        /// <param name="id">Primary key value.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The entity if found; otherwise <c>null</c>.</returns>
        Task<TEntity?> GetByIdAsync(object id, CancellationToken ct = default);

        /// <summary>
        /// Retrieves all entities of this type.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default);

        /// <summary>
        /// Returns an <see cref="IQueryable{T}"/> for composing queries.
        /// </summary>
        /// <param name="asNoTracking">
        /// When <c>true</c>, the query is configured for read-only access (no change tracking).
        /// </param>
        IQueryable<TEntity> Query(bool asNoTracking = true);

        /// <summary>
        /// Determines whether any entities match the specified predicate.
        /// </summary>
        /// <param name="predicate">Predicate used to test for existence.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);

        /// <summary>
        /// Adds an entity to the persistence context.
        /// </summary>
        /// <param name="entity">Entity to add.</param>
        /// <param name="ct">Cancellation token.</param>
        Task AddAsync(TEntity entity, CancellationToken ct = default);

        /// <summary>
        /// Marks an entity as modified in the persistence context.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        /// <param name="ct">Cancellation token.</param>
        Task UpdateAsync(TEntity entity, CancellationToken ct = default);

        /// <summary>
        /// Marks an entity for deletion in the persistence context.
        /// </summary>
        /// <param name="entity">Entity to delete.</param>
        /// <param name="ct">Cancellation token.</param>
        Task DeleteAsync(TEntity entity, CancellationToken ct = default);

        /// <summary>
        /// Persists pending changes to the database.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        Task SaveAsync(CancellationToken ct = default);
    }
}
