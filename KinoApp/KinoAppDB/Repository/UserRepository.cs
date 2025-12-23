using KinoAppDB.Entities;
using Microsoft.EntityFrameworkCore;

namespace KinoAppDB.Repository
{
    /// <summary>
    /// EF-backed repository implementation for <see cref="UserEntity"/>.
    /// </summary>
    public sealed class UserRepository : Repository<UserEntity>, IUserRepository
    {
        /// <summary>
        /// Creates a new <see cref="UserRepository"/>.
        /// </summary>
        /// <param name="scope">Database context scope used to access the current <see cref="KinoAppDbContext"/>.</param>
        public UserRepository(KinoAppDbContextScope scope) : base(scope) { }

        /// <inheritdoc />
        public Task<UserEntity?> FindByEmailAsync(string email, CancellationToken ct = default)
            => Query().FirstOrDefaultAsync(k => k.Email == email, ct);
    }
}
