using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace KinoAppDB
{
    /// <summary>
    /// Default implementation of <see cref="IKinoAppDbContextScope"/> using EF Core transactions.
    /// </summary>
    /// <remarks>
    /// This scope encapsulates DbContext lifetime and ensures repositories operate on the same
    /// context instance within a transactional boundary.
    /// </remarks>
    public sealed class KinoAppDbContextScope : IKinoAppDbContextScope
    {
        private readonly IDbContextFactory<KinoAppDbContext> _factory;
        private KinoAppDbContext? _ctx;
        private IDbContextTransaction? _tx;

        /// <summary>
        /// Creates a new <see cref="KinoAppDbContextScope"/>.
        /// </summary>
        /// <param name="factory">Factory used to create DbContext instances.</param>
        public KinoAppDbContextScope(IDbContextFactory<KinoAppDbContext> factory)
        {
            _factory = factory;
        }

        /// <inheritdoc />
        public void Create()
        {
            _tx?.Dispose();
            _ctx?.Dispose();
            _ctx = _factory.CreateDbContext();
        }

        /// <inheritdoc />
        public async Task BeginAsync(CancellationToken ct = default)
        {
            if (_ctx is null)
                Create();

            _tx = await _ctx!.Database.BeginTransactionAsync(ct);
        }

        /// <inheritdoc />
        public Task CommitAsync(CancellationToken ct = default)
            => _tx?.CommitAsync(ct) ?? Task.CompletedTask;

        /// <inheritdoc />
        public Task RollbackAsync(CancellationToken ct = default)
            => _tx?.RollbackAsync(ct) ?? Task.CompletedTask;

        /// <summary>
        /// Gets the current DbContext instance for repository access.
        /// </summary>
        internal KinoAppDbContext Current
            => _ctx ?? throw new InvalidOperationException("Database scope has not been created.");
    }
}
