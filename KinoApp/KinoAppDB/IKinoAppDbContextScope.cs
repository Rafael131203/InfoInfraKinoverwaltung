namespace KinoAppDB
{
    /// <summary>
    /// Abstraction for managing a database context scope and transaction lifecycle.
    /// </summary>
    /// <remarks>
    /// This interface decouples Core services from EF Core infrastructure details.
    /// Implementations control DbContext creation, transaction boundaries, and rollback behavior.
    /// </remarks>
    public interface IKinoAppDbContextScope
    {
        /// <summary>
        /// Creates a fresh database context for the current scope.
        /// </summary>
        void Create();

        /// <summary>
        /// Begins a database transaction within the current scope.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        Task BeginAsync(CancellationToken ct = default);

        /// <summary>
        /// Commits the active transaction.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        Task CommitAsync(CancellationToken ct = default);

        /// <summary>
        /// Rolls back the active transaction.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        Task RollbackAsync(CancellationToken ct = default);
    }
}
