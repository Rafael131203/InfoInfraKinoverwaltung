using System.Net.Http.Json;
using KinoAppCore.Services;
using KinoAppShared.DTOs.Authentication;

namespace KinoAppWeb.Services
{
    /// <summary>
    /// Client-side adapter that talks to the KinoApp API Login endpoints.
    /// </summary>
    public class ClientLoginService : IClientLoginService
    {
        private readonly HttpClient _http;

        public ClientLoginService(HttpClient http) => _http = http;

        public async Task<LoginResponseDTO?> LoginAsync(LoginRequestDTO request, CancellationToken ct = default)
        {
            var resp = await _http.PostAsJsonAsync("api/login", request, ct);
            if (!resp.IsSuccessStatusCode) return null;

            return await resp.Content.ReadFromJsonAsync<LoginResponseDTO>(cancellationToken: ct);
        }

        public async Task<LoginResponseDTO?> RefreshAsync(string refreshToken, CancellationToken ct = default)
        {
            var resp = await _http.PostAsJsonAsync("api/login/refresh",
                new RefreshRequestDTO { RefreshToken = refreshToken }, ct);
            if (!resp.IsSuccessStatusCode) return null;

            return await resp.Content.ReadFromJsonAsync<LoginResponseDTO>(cancellationToken: ct);
        }

        public async Task<RegisterResponseDTO?> RegisterAsync(RegisterRequestDTO request, CancellationToken ct = default)
        {
            var resp = await _http.PostAsJsonAsync("api/login/register", request, ct);
            if (!resp.IsSuccessStatusCode) return null;

            return await resp.Content.ReadFromJsonAsync<RegisterResponseDTO>(cancellationToken: ct);
        }

        public async Task LogoutAsync(CancellationToken ct)
        {
            try
            {
                // optional: notify backend
                var resp = await _http.PostAsync("api/login/logout", content: null, ct);
                // we don't really care about the response in this project
            }
            catch
            {
                // ignore errors – we'll still clear client session
            }
        }

    }

}
