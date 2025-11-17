using BCrypt.Net;
using KinoAppCore.Abstractions;
using Microsoft.Extensions.Configuration;

namespace KinoAppService.Security
{
    /// <summary>
    /// BCrypt password hasher; optionally supports a static "pepper" from config (Jwt:Pepper or Security:Pepper).
    /// </summary>
    public class BcryptPasswordHasher : IPasswordHasher
    {
        private readonly string? _pepper;
        private readonly int _workFactor;

        public BcryptPasswordHasher(IConfiguration cfg)
        {
            _pepper = cfg["Security:Pepper"] ?? cfg["Jwt:Pepper"]; // optional
            _workFactor = int.TryParse(cfg["Security:BCryptWorkFactor"], out var w) ? Math.Clamp(w, 10, 14) : 12;
        }

        public string Hash(string password)
        {
            var input = password + _pepper;
            return BCrypt.Net.BCrypt.HashPassword(input, workFactor: _workFactor);
        }

        public bool Verify(string password, string passwordHash)
        {
            var input = password + _pepper;
            return BCrypt.Net.BCrypt.Verify(input, passwordHash);
        }
    }
}
