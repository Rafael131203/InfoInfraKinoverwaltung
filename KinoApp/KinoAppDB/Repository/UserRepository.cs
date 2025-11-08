using KinoAppCore.Abstractions;
using KinoAppCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace KinoAppDB.Repository;

public sealed class UserRepository : IUserRepository
{
    private readonly KinoAppDbContext _db;
    public UserRepository(KinoAppDbContext db) => _db = db;

    public Task<bool> ExistsAsync(string username, CancellationToken ct)
        => _db.Users.AnyAsync(u => u.Username == username, ct);

    public Task<User?> FindAsync(string username, CancellationToken ct)
        => _db.Users.FirstOrDefaultAsync(u => u.Username == username, ct);

    public async Task AddAsync(User user, CancellationToken ct) => await _db.Users.AddAsync(user, ct);
    public Task SaveAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
