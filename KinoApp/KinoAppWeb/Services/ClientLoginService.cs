using System.Net.Http.Json;
using KinoAppShared.DTOs.Authentication;

namespace KinoAppWeb.Services
{
    /// <summary>
    /// Client-side API wrapper for the authentication endpoints.
    /// </summary>
    /// <remarks>
    /// This service is used by the Blazor client to call the backend login, refresh, register and logout routes.
    /// It returns <c>null</c> for non-success responses to keep UI flows simple.
    /// </remarks>
    public class ClientLoginService : IClientLoginService
    {
        private readonly HttpClient _http;

        /// <summary>
        /// Creates a new <see cref="ClientLoginService"/>.
        /// </summary>
        /// <param name="http">HTTP client configured with the backend base address.</param>
        public ClientLoginService(HttpClient http) => _http = http;

        /// <summary>
        /// Authenticates a user with email/password.
        /// </summary>
        /// <param name="request">Login request payload.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>
        /// A <see cref="LoginResponseDTO"/> on success; otherwise <c>null</c>.
        /// </returns>
        public async Task<LoginResponseDTO?> LoginAsync(LoginRequestDTO request, CancellationToken ct = default)
        {
            var resp = await _http.PostAsJsonAsync("api/login", request, ct);
            if (!resp.IsSuccessStatusCode) return null;

            return await resp.Content.ReadFromJsonAsync<LoginResponseDTO>(cancellationToken: ct);
        }

        /// <summary>
        /// Requests a new access token using a refresh token.
        /// </summary>
        /// <param name="refreshToken">Refresh token previously issued by the backend.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>
        /// A <see cref="LoginResponseDTO"/> containing new tokens on success; otherwise <c>null</c>.
        /// </returns>
        public async Task<LoginResponseDTO?> RefreshAsync(string refreshToken, CancellationToken ct = default)
        {
            var resp = await _http.PostAsJsonAsync(
                "api/login/refresh",
                new RefreshRequestDTO { RefreshToken = refreshToken },
                ct);

            if (!resp.IsSuccessStatusCode) return null;

            return await resp.Content.ReadFromJsonAsync<LoginResponseDTO>(cancellationToken: ct);
        }

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="request">Registration request payload.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>
        /// A <see cref="RegisterResponseDTO"/> on success; otherwise <c>null</c>.
        /// </returns>
        public async Task<RegisterResponseDTO?> RegisterAsync(RegisterRequestDTO request, CancellationToken ct = default)
        {
            var resp = await _http.PostAsJsonAsync("api/login/register", request, ct);
            if (!resp.IsSuccessStatusCode) return null;

            return await resp.Content.ReadFromJsonAsync<RegisterResponseDTO>(cancellationToken: ct);
        }

        /// <summary>
        /// Optionally notifies the backend about a logout action.
        /// </summary>
        /// <remarks>
        /// The backend uses stateless JWT, so the real logout behavior is client-side (clearing tokens).
        /// This call is best-effort and ignores network or server errors.
        /// </remarks>
        /// <param name="ct">Cancellation token.</param>
        public async Task LogoutAsync(CancellationToken ct)
        {
            try
            {
                await _http.PostAsync("api/login/logout", content: null, ct);
            }
            catch
            {
                // Ignore errors – logout is handled client-side by clearing tokens.
            }
        }
    }
}
