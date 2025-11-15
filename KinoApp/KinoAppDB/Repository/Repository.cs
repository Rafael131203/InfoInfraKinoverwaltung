using KinoAppCore.Abstractions;
using KinoAppCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace KinoAppDB.Repository;

public  class Repository<T> : IRepository<T> where T : class
{
    private readonly KinoAppDbContext _db;
    public Repository(KinoAppDbContext db) => _db = db;

    public Task<T?> GetByIdAsync(int id)
        => _db.Set<T>().FindAsync(new object[] { id }).AsTask();
    public async Task<bool> ExistsAsync(int id)
    {
        var entity = await _db.Set<T>().FindAsync(new object[] { id });
        return entity != null;
    }

    public async Task<IEnumerable<T>> GetAllAsync()
      => await _db.Set<T>().ToListAsync();

    public async Task AddAsync(T entity)
    {
        await _db.Set<T>().AddAsync(entity);
    }
    public Task UpdateAsync(T entity)
    {
        _db.Set<T>().Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity)
    {
        _db.Set<T>().Remove(entity);
        return Task.CompletedTask;
    }

    public Task SaveAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
