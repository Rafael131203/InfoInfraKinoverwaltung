using KinoAppShared.DTOs.Authentication;
using KinoAppShared.DTOs.Imdb;
using KinoAppShared.DTOs.Kinosaal;
using KinoAppShared.DTOs.Showtimes;
using KinoAppShared.DTOs.Ticket;
using Microsoft.JSInterop;
using System.Text.Json;

namespace KinoAppWeb.Services
{
    /// <summary>
    /// Holds the current client-side session state and persists it in <c>sessionStorage</c>.
    /// </summary>
    /// <remarks>
    /// The session includes authentication tokens, cached film data, selected showtime/seats, and cart items.
    /// Token refresh is handled opportunistically when an access token is close to expiring.
    /// </remarks>
    public class UserSession
    {
        private const string StorageKey = "kinoapp_session";
        private const string FilmCacheKey = "kinoapp_filmcached";
        private const string SelectedSeatsKey = "kinoapp_selected_seats";
        private const string SelectedShowtimeKey = "kinoapp_selected_showtime";
        private const string CartKey = "kinoapp_cart";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly IJSRuntime _js;
        private readonly IClientLoginService _auth;

        private bool _initialized;
        private bool _refreshInProgress;
        private LoginResponseDTO? _session;

        /// <summary>
        /// Indicates whether the current session has the Admin role.
        /// </summary>
        public bool IsAdmin => string.Equals(_session?.Role, "Admin", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Indicates whether the current session has the User role.
        /// </summary>
        public bool IsUser => string.Equals(_session?.Role, "User", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Display name for UI usage. Falls back to email if first name is not present.
        /// </summary>
        public string? DisplayName => string.IsNullOrWhiteSpace(_session?.Vorname) ? _session?.Email : _session?.Vorname;

        /// <summary>
        /// Currently selected showtime (movie context + showtime data).
        /// </summary>
        public SelectedShowtimeDto? SelectedShowtime { get; private set; }

        /// <summary>
        /// Currently selected seats for the active selection flow.
        /// </summary>
        public List<SelectedSeatClientDto> SelectedSeats { get; private set; } = new();

        /// <summary>
        /// Current cart contents. Each item represents one selected seat plus movie/showtime context.
        /// </summary>
        public List<CartItemDto> CartItems { get; private set; } = new();

        /// <summary>
        /// Number of items in the cart.
        /// </summary>
        public int CartCount => CartItems.Count;

        /// <summary>
        /// Event raised whenever cart state changes.
        /// </summary>
        public event Action? CartChanged;

        /// <summary>
        /// Cached film list to avoid repeated API calls during a session.
        /// </summary>
        public List<FilmDto> CachedFilms { get; private set; } = new();

        /// <summary>
        /// Creates a new <see cref="UserSession"/>.
        /// </summary>
        /// <param name="js">JS runtime used for sessionStorage access.</param>
        /// <param name="auth">Authentication client used for refresh/logout operations.</param>
        public UserSession(IJSRuntime js, IClientLoginService auth)
        {
            _js = js;
            _auth = auth;
        }

        /// <summary>
        /// Indicates whether a user is authenticated (access token present).
        /// </summary>
        public bool IsAuthenticated => _session?.Token is not null;

        /// <summary>
        /// Current login response payload backing this session.
        /// </summary>
        public LoginResponseDTO? Current => _session;

        /// <summary>
        /// Current access token string or null.
        /// </summary>
        public string? CurrentAccessToken => _session?.Token?.Token;

        /// <summary>
        /// Loads session state from sessionStorage once per app lifetime.
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_initialized) return;

            try
            {
                var json = await _js.InvokeAsync<string?>("sessionStorage.getItem", StorageKey);
                if (!string.IsNullOrWhiteSpace(json))
                    _session = JsonSerializer.Deserialize<LoginResponseDTO>(json, JsonOptions);

                var filmJson = await _js.InvokeAsync<string?>("sessionStorage.getItem", FilmCacheKey);
                CachedFilms = !string.IsNullOrWhiteSpace(filmJson)
                    ? JsonSerializer.Deserialize<List<FilmDto>>(filmJson, JsonOptions) ?? new()
                    : new();

                var stJson = await _js.InvokeAsync<string?>("sessionStorage.getItem", SelectedShowtimeKey);
                if (!string.IsNullOrWhiteSpace(stJson))
                    SelectedShowtime = JsonSerializer.Deserialize<SelectedShowtimeDto>(stJson, JsonOptions);

                var selSeatsJson = await _js.InvokeAsync<string?>("sessionStorage.getItem", SelectedSeatsKey);
                SelectedSeats = !string.IsNullOrWhiteSpace(selSeatsJson)
                    ? JsonSerializer.Deserialize<List<SelectedSeatClientDto>>(selSeatsJson, JsonOptions) ?? new()
                    : new();

                var cartJson = await _js.InvokeAsync<string?>("sessionStorage.getItem", CartKey);
                CartItems = !string.IsNullOrWhiteSpace(cartJson)
                    ? JsonSerializer.Deserialize<List<CartItemDto>>(cartJson, JsonOptions) ?? new()
                    : new();
            }
            catch
            {
                CartItems = new();
                CachedFilms = new();
            }

            _initialized = true;
            RaiseCartChanged();
        }

        /// <summary>
        /// Stores a newly authenticated session in memory and sessionStorage.
        /// </summary>
        public async Task SetSessionAsync(LoginResponseDTO dto)
        {
            _session = dto;
            var json = JsonSerializer.Serialize(dto, JsonOptions);
            await _js.InvokeVoidAsync("sessionStorage.setItem", StorageKey, json);
        }

        /// <summary>
        /// Clears all session-related state and removes persisted values from sessionStorage.
        /// </summary>
        private async Task ClearAsync()
        {
            _session = null;
            _refreshInProgress = false;

            CachedFilms.Clear();
            SelectedSeats.Clear();
            SelectedShowtime = null;
            CartItems.Clear();

            await _js.InvokeVoidAsync("sessionStorage.removeItem", StorageKey);
            await _js.InvokeVoidAsync("sessionStorage.removeItem", FilmCacheKey);
            await _js.InvokeVoidAsync("sessionStorage.removeItem", SelectedShowtimeKey);
            await _js.InvokeVoidAsync("sessionStorage.removeItem", SelectedSeatsKey);
            await _js.InvokeVoidAsync("sessionStorage.removeItem", CartKey);

            RaiseCartChanged();
        }

        /// <summary>
        /// Logs out the current user and clears all local session data.
        /// </summary>
        public async Task LogoutAsync(CancellationToken ct = default)
        {
            try { await _auth.LogoutAsync(ct); }
            catch { }
            finally { await ClearAsync(); }
        }

        /// <summary>
        /// Returns a valid access token, refreshing it if it is close to expiration.
        /// </summary>
        public async Task<string?> GetValidAccessTokenAsync(CancellationToken ct = default)
        {
            if (_session?.Token == null)
                return null;

            if ((_session.Token.ExpiresAt - DateTime.UtcNow).TotalSeconds < 120)
                await RefreshAsync(ct);

            return _session?.Token.Token;
        }

        /// <summary>
        /// Refreshes the current session tokens using the refresh token.
        /// </summary>
        private async Task RefreshAsync(CancellationToken ct)
        {
            if (_refreshInProgress || _session?.RefreshToken == null)
                return;

            try
            {
                _refreshInProgress = true;

                var refreshed = await _auth.RefreshAsync(_session.RefreshToken.Token, ct);
                if (refreshed == null)
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

        /// <summary>
        /// Sets the currently selected showtime and persists it to sessionStorage.
        /// </summary>
        public async Task SetSelectedShowtimeAsync(int movieId, string movieTitle, string? posterUrl, ShowtimeDto showtime)
        {
            SelectedShowtime = new SelectedShowtimeDto
            {
                MovieId = movieId,
                MovieTitle = movieTitle,
                PosterUrl = posterUrl,
                Showtime = showtime
            };

            var json = JsonSerializer.Serialize(SelectedShowtime, JsonOptions);
            await _js.InvokeVoidAsync("sessionStorage.setItem", SelectedShowtimeKey, json);
        }

        /// <summary>
        /// Sets the current selected seats and persists them to sessionStorage.
        /// </summary>
        public async Task SetSelectedSeatsAsync(List<SelectedSeatClientDto> seats)
        {
            SelectedSeats = seats ?? new();
            var json = JsonSerializer.Serialize(SelectedSeats, JsonOptions);
            await _js.InvokeVoidAsync("sessionStorage.setItem", SelectedSeatsKey, json);
        }

        /// <summary>
        /// Adds seat selections to the cart and persists the cart to sessionStorage.
        /// </summary>
        public async Task AddToCartAsync(
            IEnumerable<SelectedSeatClientDto> seats,
            int movieId,
            string movieTitle,
            string? posterUrl,
            ShowtimeDto showtime)
        {
            foreach (var seat in seats)
            {
                CartItems.Add(new CartItemDto
                {
                    Seat = seat,
                    MovieId = movieId,
                    MovieTitle = movieTitle,
                    PosterUrl = posterUrl,
                    Showtime = showtime
                });
            }

            var json = JsonSerializer.Serialize(CartItems, JsonOptions);
            await _js.InvokeVoidAsync("sessionStorage.setItem", CartKey, json);

            RaiseCartChanged();
        }

        /// <summary>
        /// Clears the cart and removes its persisted value from sessionStorage.
        /// </summary>
        public async Task ClearCartAsync()
        {
            CartItems.Clear();
            await _js.InvokeVoidAsync("sessionStorage.removeItem", CartKey);
            RaiseCartChanged();
        }

        /// <summary>
        /// Clears selected seats and removes persisted value from sessionStorage.
        /// </summary>
        public async Task ClearSelectedSeatsAsync()
        {
            SelectedSeats.Clear();
            await _js.InvokeVoidAsync("sessionStorage.removeItem", SelectedSeatsKey);
        }

        /// <summary>
        /// Clears the selected showtime and removes persisted value from sessionStorage.
        /// </summary>
        public async Task ClearSelectedShowtimeAsync()
        {
            SelectedShowtime = null;
            await _js.InvokeVoidAsync("sessionStorage.removeItem", SelectedShowtimeKey);
        }

        /// <summary>
        /// Returns cached films if available; otherwise loads them via the provided API client and caches them.
        /// </summary>
        public async Task<List<FilmDto>> GetFilmsAsync(ImdbApiClient client, CancellationToken ct = default)
        {
            await InitializeAsync();

            if (CachedFilms is { Count: > 0 })
                return CachedFilms;

            var films = await client.GetLocalFilmsAsync(ct);
            await StoreFilmsAsync(films);

            return CachedFilms;
        }

        /// <summary>
        /// Stores a film list in memory and sessionStorage.
        /// </summary>
        public async Task StoreFilmsAsync(List<FilmDto> films)
        {
            CachedFilms = films ?? new List<FilmDto>();

            var json = JsonSerializer.Serialize(CachedFilms, JsonOptions);
            await _js.InvokeVoidAsync("sessionStorage.setItem", FilmCacheKey, json);
        }

        private void RaiseCartChanged() => CartChanged?.Invoke();
    }
}
