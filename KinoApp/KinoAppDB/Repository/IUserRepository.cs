using KinoAppDB.Entities;

namespace KinoAppDB.Repository
{
    /// <summary>
    /// Repository contract for user persistence operations.
    /// </summary>
    public interface IUserRepository : IRepository<UserEntity>
    {
        /// <summary>
        /// Finds a user by email address.
        /// </summary>
        /// <param name="email">Email address to search for.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The user if found; otherwise <c>null</c>.</returns>
        Task<UserEntity?> FindByEmailAsync(string email, CancellationToken ct = default);
    }
}
