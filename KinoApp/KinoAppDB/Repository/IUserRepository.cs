using KinoAppDB.Entities;

namespace KinoAppDB.Repository;

public interface IUserRepository : IRepository<UserEntity>
{
    Task<UserEntity?> FindByEmailAsync(string email, CancellationToken ct = default);
}
