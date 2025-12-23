using BCrypt.Net;
using KinoAppCore.Abstractions;
using Microsoft.Extensions.Configuration;

namespace KinoAppService.Security
{
    /// <summary>
    /// Password hasher implementation using BCrypt.
    /// </summary>
    /// <remarks>
    /// Supports an optional "pepper" value loaded from configuration (<c>Security:Pepper</c> or <c>Jwt:Pepper</c>).
    /// The pepper is appended to the provided password before hashing/verifying and should be stored as a secret.
    /// </remarks>
    public sealed class BcryptPasswordHasher : IPasswordHasher
    {
        private readonly string? _pepper;
        private readonly int _workFactor;

        /// <summary>
        /// Creates a new <see cref="BcryptPasswordHasher"/> instance.
        /// </summary>
        /// <param name="cfg">Application configuration used to resolve pepper and work factor settings.</param>
        public BcryptPasswordHasher(IConfiguration cfg)
        {
            _pepper = cfg["Security:Pepper"] ?? cfg["Jwt:Pepper"];
            _workFactor = int.TryParse(cfg["Security:BCryptWorkFactor"], out var w)
                ? Math.Clamp(w, 10, 14)
                : 12;
        }

        /// <summary>
        /// Hashes the provided password (optionally combined with a pepper) using BCrypt.
        /// </summary>
        /// <param name="password">Plain text password.</param>
        /// <returns>A BCrypt hash including salt and work factor.</returns>
        public string Hash(string password)
        {
            var input = password + _pepper;
            return BCrypt.Net.BCrypt.HashPassword(input, workFactor: _workFactor);
        }

        /// <summary>
        /// Verifies a plain text password against a stored BCrypt hash.
        /// </summary>
        /// <param name="password">Plain text password.</param>
        /// <param name="passwordHash">Stored BCrypt hash.</param>
        /// <returns><c>true</c> if the password matches; otherwise <c>false</c>.</returns>
        public bool Verify(string password, string passwordHash)
        {
            var input = password + _pepper;
            return BCrypt.Net.BCrypt.Verify(input, passwordHash);
        }
    }
}
