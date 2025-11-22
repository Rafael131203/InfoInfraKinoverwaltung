using KinoAppShared.DTOs.Authentication;

namespace KinoAppCore.Abstractions
{
    public interface ITokenService
    {
        TokenInfoDTO GenerateAccessToken(long userId, string email, string vorname, string nachname, string role);
        TokenInfoDTO GenerateRefreshToken(long userId, string email);
        (long userId, string email)? ValidateRefreshToken(string refreshToken);
    }

}
