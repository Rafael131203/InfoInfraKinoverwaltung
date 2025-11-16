namespace KinoAppCore.Abstractions;

/// <summary>
/// Abstraction for opening a persistence scope and handling transactions.
/// Implemented in KinoAppDB; consumed in services from Core.
/// </summary>
public interface IDbContextScope
{
    void Create(); // ensure a fresh DbContext/scope

    Task BeginAsync(CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}
