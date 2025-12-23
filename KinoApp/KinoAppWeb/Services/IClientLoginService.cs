using KinoAppShared.DTOs.Authentication;

namespace KinoAppWeb.Services
{
    /// <summary>
    /// Abstraction for client-side authentication API calls.
    /// </summary>
    public interface IClientLoginService
    {
        /// <summary>
        /// Authenticates a user with email/password.
        /// </summary>
        Task<LoginResponseDTO?> LoginAsync(LoginRequestDTO request, CancellationToken ct = default);

        /// <summary>
        /// Refreshes tokens using a refresh token.
        /// </summary>
        Task<LoginResponseDTO?> RefreshAsync(string refreshToken, CancellationToken ct = default);

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        Task<RegisterResponseDTO?> RegisterAsync(RegisterRequestDTO request, CancellationToken ct = default);

        /// <summary>
        /// Logs out the current session (best-effort backend notification).
        /// </summary>
        Task LogoutAsync(CancellationToken ct);
    }
}
