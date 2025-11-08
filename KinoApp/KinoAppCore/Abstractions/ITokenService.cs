namespace KinoAppCore.Abstractions;
public interface ITokenService
{
    (string token, DateTime expiresUtc) CreateToken(string username, IEnumerable<string>? roles = null);
}
