using BCrypt.Net;
using KinoAppCore.Abstractions;
using KinoAppCore.Entities;

namespace KinoAppCore.Services;
public interface IAuthService
{
    Task<(bool ok, string? error)> RegisterAsync(string username, string password, CancellationToken ct);
    Task<(bool ok, string? error, string token, DateTime expiresUtc)> LoginAsync(string username, string password, CancellationToken ct);
}

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly ITokenService _tokens;

    public AuthService(IUserRepository users, ITokenService tokens)
    { _users = users; _tokens = tokens; }

    public async Task<(bool ok, string? error)> RegisterAsync(string username, string password, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return (false, "Username and password are required.");

        if (await _users.ExistsAsync(username, ct))
            return (false, "Username already exists.");

        var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
        await _users.AddAsync(new User(username, hash), ct);
        await _users.SaveAsync(ct);
        return (true, null);
    }

    public async Task<(bool ok, string? error, string token, DateTime expiresUtc)> LoginAsync(string username, string password, CancellationToken ct)
    {
        var user = await _users.FindAsync(username, ct);
        if (user is null) return (false, "Invalid credentials.", "", DateTime.MinValue);

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return (false, "Invalid credentials.", "", DateTime.MinValue);

        var roles = (user.RolesCsv ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
        var (token, exp) = _tokens.CreateToken(user.Username, roles);
        return (true, null, token, exp);
    }
}
