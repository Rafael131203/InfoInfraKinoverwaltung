using KinoAppCore.Entities;
namespace KinoAppCore.Abstractions;
public interface IUserRepository
{
    Task<bool> ExistsAsync(string username, CancellationToken ct);
    Task<User?> FindAsync(string username, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
}
