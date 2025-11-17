using KinoAppShared.DTOs.Authentication;

namespace KinoAppCore.Services
{
    public interface ILoginService
    {
        Task<LoginResponseDTO?> AuthenticateAsync(LoginRequestDTO request, CancellationToken ct = default);
        Task<LoginResponseDTO?> RefreshAsync(string refreshToken, CancellationToken ct = default);
    }
}
