using KinoAppShared.DTOs.Authentication;

namespace KinoAppCore.Services
{
    /// <summary>
    /// Handles authentication flows including login, token refresh, and user registration.
    /// </summary>
    public interface ILoginService
    {
        /// <summary>
        /// Authenticates a user using the provided credentials.
        /// </summary>
        /// <param name="request">Login request containing email and password.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>
        /// A populated response containing tokens and user details if authentication succeeds;
        /// otherwise <c>null</c>.
        /// </returns>
        Task<LoginResponseDTO?> AuthenticateAsync(LoginRequestDTO request, CancellationToken ct = default);

        /// <summary>
        /// Issues a new access/refresh token pair based on a valid refresh token.
        /// </summary>
        /// <param name="refreshToken">Refresh token previously issued by the system.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>
        /// A populated response containing new tokens and user details if the refresh token is valid;
        /// otherwise <c>null</c>.
        /// </returns>
        Task<LoginResponseDTO?> RefreshAsync(string refreshToken, CancellationToken ct = default);

        /// <summary>
        /// Registers a new user and returns the created user data.
        /// </summary>
        /// <param name="dto">Registration request.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The registration response DTO.</returns>
        Task<RegisterResponseDTO> RegisterAsync(RegisterRequestDTO dto, CancellationToken ct = default);
    }
}
