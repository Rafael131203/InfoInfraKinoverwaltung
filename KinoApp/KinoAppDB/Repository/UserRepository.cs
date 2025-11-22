using KinoAppDB.Entities;
using Microsoft.EntityFrameworkCore;

namespace KinoAppDB.Repository;

public sealed class UserRepository : Repository<UserEntity>, IUserRepository
{
    public UserRepository(KinoAppDbContextScope scope) : base(scope) { }

    public Task<UserEntity?> FindByEmailAsync(string email, CancellationToken ct = default)
        => Query().FirstOrDefaultAsync(k => k.Email == email, ct);
}
