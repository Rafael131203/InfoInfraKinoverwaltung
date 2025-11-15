using KinoAppCore.Entities;
namespace KinoAppCore.Abstractions;
public interface IRepository<T>
{
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    Task UpdateAsync (T entity);
    Task DeleteAsync(T entity);
    Task SaveAsync(CancellationToken ct);
}
