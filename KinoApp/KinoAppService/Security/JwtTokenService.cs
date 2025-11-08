using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using KinoAppCore.Abstractions;
using Microsoft.IdentityModel.Tokens;

namespace KinoAppService.Security;

public sealed class JwtTokenService : ITokenService
{
    private readonly string _issuer, _audience;
    private readonly SymmetricSecurityKey _key;
    private readonly TimeSpan _lifetime;

    public JwtTokenService(string issuer, string audience, SymmetricSecurityKey key, TimeSpan lifetime)
    {
        _issuer = issuer; _audience = audience; _key = key; _lifetime = lifetime;
    }

    public (string token, DateTime expiresUtc) CreateToken(string username, System.Collections.Generic.IEnumerable<string>? roles = null)
    {
        var now = DateTime.UtcNow;
        var claims = new[] { new Claim(ClaimTypes.Name, username) }.ToList();
        if (roles != null) claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
        var exp = now.Add(_lifetime);
        var jwt = new JwtSecurityToken(_issuer, _audience, claims, now, exp, creds);
        return (new JwtSecurityTokenHandler().WriteToken(jwt), exp);
    }
}
