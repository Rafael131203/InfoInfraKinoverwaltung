using KinoAppShared.DTOs.Authentication;
using KinoAppShared.DTOs.Imdb;
using KinoAppShared.DTOs.Showtimes;
using KinoAppShared.DTOs.Ticket;
using Microsoft.JSInterop;
using System.Text.Json;

namespace KinoAppWeb.Services
{
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

        public bool IsAdmin => string.Equals(_session?.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        public bool IsUser => string.Equals(_session?.Role, "User", StringComparison.OrdinalIgnoreCase);
        public string? DisplayName => string.IsNullOrWhiteSpace(_session?.Vorname) ? _session?.Email : _session?.Vorname;

        public SelectedShowtimeDto? SelectedShowtime { get; private set; }
        public List<SelectedSeatClientDto> SelectedSeats { get; private set; } = new();

        // 🔥 THE ONLY REAL CART NOW
        public List<CartItemDto> CartItems { get; private set; } = new();

        public int CartCount => CartItems.Count;

        public event Action? CartChanged;

        private void RaiseCartChanged() => CartChanged?.Invoke();

        public List<FilmDto> CachedFilms { get; private set; } = new();

        public UserSession(IJSRuntime js, IClientLoginService auth)
        {
            _js = js;
            _auth = auth;
        }

        public bool IsAuthenticated => _session?.Token is not null;
        public LoginResponseDTO? Current => _session;
        public string? CurrentAccessToken => _session?.Token?.Token;

        public async Task InitializeAsync()
        {
            if (_initialized) return;

            try
            {
                // session
                var json = await _js.InvokeAsync<string?>("sessionStorage.getItem", StorageKey);
                if (!string.IsNullOrWhiteSpace(json))
                    _session = JsonSerializer.Deserialize<LoginResponseDTO>(json, JsonOptions);

                // films
                var filmJson = await _js.InvokeAsync<string?>("sessionStorage.getItem", FilmCacheKey);
                CachedFilms = !string.IsNullOrWhiteSpace(filmJson)
                    ? JsonSerializer.Deserialize<List<FilmDto>>(filmJson, JsonOptions) ?? new()
                    : new();

                // selected showtime
                var stJson = await _js.InvokeAsync<string?>("sessionStorage.getItem", SelectedShowtimeKey);
                if (!string.IsNullOrWhiteSpace(stJson))
                    SelectedShowtime = JsonSerializer.Deserialize<SelectedShowtimeDto>(stJson, JsonOptions);

                // selected seats
                var selSeatsJson = await _js.InvokeAsync<string?>("sessionStorage.getItem", SelectedSeatsKey);
                SelectedSeats = !string.IsNullOrWhiteSpace(selSeatsJson)
                    ? JsonSerializer.Deserialize<List<SelectedSeatClientDto>>(selSeatsJson, JsonOptions) ?? new()
                    : new();

                // cart items
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

        public async Task SetSessionAsync(LoginResponseDTO dto)
        {
            _session = dto;
            var json = JsonSerializer.Serialize(dto, JsonOptions);
            await _js.InvokeVoidAsync("sessionStorage.setItem", StorageKey, json);
        }

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

        public async Task LogoutAsync(CancellationToken ct = default)
        {
            try { await _auth.LogoutAsync(ct); }
            catch { }
            finally { await ClearAsync(); }
        }

        public async Task<string?> GetValidAccessTokenAsync(CancellationToken ct = default)
        {
            if (_session?.Token == null)
                return null;

            if ((_session.Token.ExpiresAt - DateTime.UtcNow).TotalSeconds < 120)
                await RefreshAsync(ct);

            return _session?.Token.Token;
        }

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

        public async Task SetSelectedSeatsAsync(List<SelectedSeatClientDto> seats)
        {
            SelectedSeats = seats ?? new();
            var json = JsonSerializer.Serialize(SelectedSeats, JsonOptions);
            await _js.InvokeVoidAsync("sessionStorage.setItem", SelectedSeatsKey, json);
        }

        // 🔥 Add seats to the real cart
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

        public async Task ClearCartAsync()
        {
            CartItems.Clear();
            await _js.InvokeVoidAsync("sessionStorage.removeItem", CartKey);
        }

        public async Task ClearSelectedSeatsAsync()
        {
            SelectedSeats.Clear();
            await _js.InvokeVoidAsync("sessionStorage.removeItem", SelectedSeatsKey);
        }

        public async Task ClearSelectedShowtimeAsync()
        {
            SelectedShowtime = null;
            await _js.InvokeVoidAsync("sessionStorage.removeItem", SelectedShowtimeKey);
        }

        public async Task<List<FilmDto>> GetFilmsAsync(ImdbApiClient client, CancellationToken ct = default)
        {
            await InitializeAsync();

            if (CachedFilms is { Count: > 0 })
                return CachedFilms;

            var films = await client.GetLocalFilmsAsync(ct);
            await StoreFilmsAsync(films);

            return CachedFilms;
        }

        public async Task StoreFilmsAsync(List<FilmDto> films)
        {
            CachedFilms = films ?? new List<FilmDto>();

            var json = JsonSerializer.Serialize(CachedFilms, JsonOptions);
            await _js.InvokeVoidAsync("sessionStorage.setItem", FilmCacheKey, json);
        }


    }
}
