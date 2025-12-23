using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KinoAppCore.Abstractions;
using KinoAppShared.DTOs.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace KinoAppService.Security
{
    /// <summary>
    /// Token service for issuing and validating JWT access and refresh tokens.
    /// </summary>
    /// <remarks>
    /// Access tokens are short-lived and include identity + role claims.
    /// Refresh tokens are used to obtain new tokens and are validated for issuer, audience, lifetime,
    /// and signature, and additionally checked for a <c>type=refresh</c> claim.
    /// </remarks>
    public sealed class JwtTokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;
        private readonly JwtSecurityTokenHandler _handler = new();

        /// <summary>
        /// Creates a new <see cref="JwtTokenService"/>.
        /// </summary>
        /// <param name="config">Application configuration providing issuer, audience, and signing key.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <c>Jwt:SigningKey</c> is missing or too short for secure HMAC signing.
        /// </exception>
        public JwtTokenService(IConfiguration config)
        {
            _config = config;

            var keyString = _config["Jwt:SigningKey"]
                ?? throw new InvalidOperationException("Jwt:SigningKey missing");

            if (keyString.Length < 32)
                throw new InvalidOperationException("Signing key must be at least 32 characters.");

            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        }

        /// <summary>
        /// Generates a signed JWT access token for the given user.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        /// <param name="email">User email address.</param>
        /// <param name="vorname">User first name.</param>
        /// <param name="nachname">User last name.</param>
        /// <param name="role">User role used for authorization.</param>
        /// <returns>Token metadata including creation time, expiry, and the token string.</returns>
        public TokenInfoDTO GenerateAccessToken(long userId, string email, string vorname, string nachname, string role)
        {
            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(15);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim("vorname", vorname),
                new Claim("nachname", nachname),
                new Claim(ClaimTypes.Role, role),
                new Claim("type", "access")
            };

            var token = CreateTokenInternal(claims, expires);

            return new TokenInfoDTO
            {
                Token = token,
                CreatedAt = now,
                ExpiresAt = expires,
                Claims = claims.ToDictionary(c => c.Type, c => c.Value)
            };
        }

        /// <summary>
        /// Generates a signed JWT refresh token for the given user.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        /// <param name="email">User email address.</param>
        /// <returns>Token metadata including creation time, expiry, and the token string.</returns>
        public TokenInfoDTO GenerateRefreshToken(long userId, string email)
        {
            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(30);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim("type", "refresh")
            };

            var token = CreateTokenInternal(claims, expires);

            return new TokenInfoDTO
            {
                Token = token,
                CreatedAt = now,
                ExpiresAt = expires,
                Claims = claims.ToDictionary(c => c.Type, c => c.Value)
            };
        }

        /// <summary>
        /// Validates a refresh token and returns the embedded user identity if valid.
        /// </summary>
        /// <param name="refreshToken">Refresh token to validate.</param>
        /// <returns>
        /// A tuple of <c>(userId, email)</c> when valid; otherwise <c>null</c>.
        /// </returns>
        public (long userId, string email)? ValidateRefreshToken(string refreshToken)
        {
            try
            {
                var principal = _handler.ValidateToken(
                    refreshToken,
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = _config["Jwt:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = _config["Jwt:Audience"],
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = _key,
                        ClockSkew = TimeSpan.FromMinutes(1)
                    },
                    out var validatedToken);

                if (validatedToken is not JwtSecurityToken jwt)
                    return null;

                var type = jwt.Claims.FirstOrDefault(c => c.Type == "type")?.Value;
                if (!string.Equals(type, "refresh", StringComparison.OrdinalIgnoreCase))
                    return null;

                var sub =
                    principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
                    principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var email =
                    principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value ??
                    principal.FindFirst(ClaimTypes.Email)?.Value;

                if (string.IsNullOrWhiteSpace(sub) || string.IsNullOrWhiteSpace(email))
                    return null;

                if (!long.TryParse(sub, out var id))
                    return null;

                return (id, email);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Creates and signs a JWT with the configured issuer/audience and the provided claims.
        /// </summary>
        private string CreateTokenInternal(IEnumerable<Claim> claims, DateTime expires)
        {
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return _handler.WriteToken(jwt);
        }
    }
}
