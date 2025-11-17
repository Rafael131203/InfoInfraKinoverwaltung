using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KinoAppCore.Abstractions;
using KinoAppShared.DTOs.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace KinoAppService.Security
{
    public class JwtTokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;
        private readonly JwtSecurityTokenHandler _handler = new();

        public JwtTokenService(IConfiguration config)
        {
            _config = config;

            var keyString = _config["Jwt:SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey missing");

            if (keyString.Length < 32)
                throw new InvalidOperationException("Signing key must be at least 32 characters.");

            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        }

        public TokenInfoDTO GenerateAccessToken(long userId, string email, string vorname, string nachname)
        {
            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(15);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim("vorname", vorname),
                new Claim("nachname", nachname),
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

            // must be a JWT + must be a REFRESH token
            if (validatedToken is not JwtSecurityToken jwt)
                return null;

            var type = jwt.Claims.FirstOrDefault(c => c.Type == "type")?.Value;
            if (!string.Equals(type, "refresh", StringComparison.OrdinalIgnoreCase))
                return null;

            // Try multiple possible claim types for id/email
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
            // any validation/parsing error → treat as invalid token
            return null;
        }
    }


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
