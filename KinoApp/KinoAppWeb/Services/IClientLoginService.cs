using KinoAppShared.DTOs.Authentication;

namespace KinoAppWeb.Services
{
    public interface IClientLoginService
    {
        Task<LoginResponseDTO?> LoginAsync(LoginRequestDTO request, CancellationToken ct = default);
        Task<LoginResponseDTO?> RefreshAsync(string refreshToken, CancellationToken ct = default);
        Task<RegisterResponseDTO?> RegisterAsync(RegisterRequestDTO request, CancellationToken ct = default);
        Task LogoutAsync(CancellationToken ct);
    }
}
