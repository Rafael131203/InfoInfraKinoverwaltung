using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace KinoAppDB;

public sealed class KinoAppDbContextScope : IKinoAppDbContextScope
{
    private readonly IDbContextFactory<KinoAppDbContext> _factory;
    private KinoAppDbContext? _ctx;
    private IDbContextTransaction? _tx;

    public KinoAppDbContextScope(IDbContextFactory<KinoAppDbContext> factory) => _factory = factory;

    public void Create()
    {
        _tx?.Dispose();
        _ctx?.Dispose();
        _ctx = _factory.CreateDbContext();
    }

    public async Task BeginAsync(CancellationToken ct = default)
    {
        if (_ctx is null) Create();
        _tx = await _ctx!.Database.BeginTransactionAsync(ct);
    }

    public Task CommitAsync(CancellationToken ct = default) => _tx?.CommitAsync(ct) ?? Task.CompletedTask;
    public Task RollbackAsync(CancellationToken ct = default) => _tx?.RollbackAsync(ct) ?? Task.CompletedTask;

    // internal accessor for repositories
    internal KinoAppDbContext Current => _ctx ?? throw new InvalidOperationException("Scope not created.");
}
