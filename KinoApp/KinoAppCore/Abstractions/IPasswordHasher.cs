namespace KinoAppCore.Abstractions
{
    /// <summary>
    /// Provides password hashing and verification functionality for the application.
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Hashes the specified password using the configured hashing strategy.
        /// </summary>
        /// <param name="password">The plain text password to hash.</param>
        /// <returns>A string containing the resulting password hash (including any required metadata such as salt/cost).</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="password"/> is null or empty.</exception>
        string Hash(string password);

        /// <summary>
        /// Verifies that the specified password matches the provided password hash.
        /// </summary>
        /// <param name="password">The plain text password to verify.</param>
        /// <param name="passwordHash">The stored password hash to compare against.</param>
        /// <returns><c>true</c> if the password matches the hash; otherwise <c>false</c>.</returns>
        bool Verify(string password, string passwordHash);
    }
}
