using System.Text.Json;
using KinoAppShared.DTOs.Authentication;
using Microsoft.JSInterop;

namespace KinoAppWeb.Services
{
    public class UserSession
    {
        private const string StorageKey = "kinoapp_session";
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly IJSRuntime _js;
        private readonly IClientLoginService _auth;

        private bool _initialized;
        private bool _refreshInProgress;
        private LoginResponseDTO? _session;

        public UserSession(IJSRuntime js, IClientLoginService auth)
        {
            _js = js;
            _auth = auth;
        }

        /// <summary>True if there is a current session with tokens.</summary>
        public bool IsAuthenticated => _session?.Token is not null;

        /// <summary>The current full login payload (tokens + user info).</summary>
        public LoginResponseDTO? Current => _session;

        /// <summary>The current access token string (may be null).</summary>
        public string? CurrentAccessToken => _session?.Token?.Token;

        /// <summary>Load session from sessionStorage once at startup.</summary>
        public async Task InitializeAsync()
        {
            if (_initialized) return;

            try
            {
                var json = await _js.InvokeAsync<string?>("sessionStorage.getItem", StorageKey);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    _session = JsonSerializer.Deserialize<LoginResponseDTO>(json, JsonOptions);
                }
            }
            catch
            {
                // ignore, treat as no session
            }

            _initialized = true;
        }

        /// <summary>Stores the login payload in memory and sessionStorage.</summary>
        public async Task SetSessionAsync(LoginResponseDTO dto)
        {
            _session = dto;

            var json = JsonSerializer.Serialize(dto, JsonOptions);
            await _js.InvokeVoidAsync("sessionStorage.setItem", StorageKey, json);
        }

        /// <summary>Clears all session data (tokens + flags + storage).</summary>
        public async Task ClearAsync()
        {
            _session = null;
            _refreshInProgress = false;

            await _js.InvokeVoidAsync("sessionStorage.removeItem", StorageKey);
        }

        /// <summary>
        /// Performs a full logout: optionally notifies the API and then clears local session state.
        /// </summary>
        public async Task LogoutAsync(CancellationToken ct = default)
        {
            try
            {
                // Optional: tell backend we're logging out (no-op on your current API,
                // but ready if you later add server-side refresh token invalidation)
                await _auth.LogoutAsync(ct);
            }
            catch
            {
                // Ignore API errors, local logout still proceeds
            }
            finally
            {
                await ClearAsync();
            }
        }

        /// <summary>
        /// Returns a valid access token. If the current one is expired or about to expire,
        /// tries to refresh using the refresh token.
        /// </summary>
        public async Task<string?> GetValidAccessTokenAsync(CancellationToken ct = default)
        {
            if (_session?.Token is null)
                return null;

            var now = DateTime.UtcNow;
            var expires = _session.Token.ExpiresAt;

            // How much lifetime left?
            var secondsLeft = (expires - now).TotalSeconds;

            // If expired or less than 2 minutes left, refresh
            if (secondsLeft <= 0 || secondsLeft < 120)
            {
                await RefreshAsync(ct);
            }

            return _session?.Token.Token;
        }

        private async Task RefreshAsync(CancellationToken ct)
        {
            if (_refreshInProgress) return; // avoid concurrent refreshes

            if (_session?.RefreshToken == null)
            {
                await ClearAsync();
                return;
            }

            try
            {
                _refreshInProgress = true;

                // IClientLoginService.RefreshAsync must return a fresh LoginResponseDTO
                var refreshed = await _auth.RefreshAsync(_session.RefreshToken.Token, ct);
                if (refreshed is null)
                {
                    await ClearAsync();
                    return;
                }

                await SetSessionAsync(refreshed);
            }
            finally
            {
                _refreshInProgress = false;
            }
        }
    }
}
