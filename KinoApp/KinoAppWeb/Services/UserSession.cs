using KinoAppShared.DTOs.Authentication;
using KinoAppShared.DTOs.Imdb;
using KinoAppShared.DTOs.Showtimes;
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
        private const string CartKey = "kinoapp_cart";         // 🔥 NEW

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

        // 🔥 NEW: persistent cart
        public List<SelectedSeatClientDto> Cart { get; private set; } = new();

        /// <summary>Cached films for this browser session, mirrored in sessionStorage.</summary>
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
                // auth/session
                var json = await _js.InvokeAsync<string?>("sessionStorage.getItem", StorageKey);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    _session = JsonSerializer.Deserialize<LoginResponseDTO>(json, JsonOptions);
                }

                // film cache
                var filmJson = await _js.InvokeAsync<string?>("sessionStorage.getItem", FilmCacheKey);
                if (!string.IsNullOrWhiteSpace(filmJson))
                {
                    var films = JsonSerializer.Deserialize<List<FilmDto>>(filmJson, JsonOptions);
                    CachedFilms = films ?? new List<FilmDto>();
                }
                else
                {
                    CachedFilms = new List<FilmDto>();
                }

                // selected showtime
                var selectedShowtimeJson = await _js.InvokeAsync<string?>("sessionStorage.getItem", SelectedShowtimeKey);
                if (!string.IsNullOrWhiteSpace(selectedShowtimeJson))
                {
                    SelectedShowtime = JsonSerializer.Deserialize<SelectedShowtimeDto>(selectedShowtimeJson, JsonOptions);
                }

                // selected seats (current selection)
                var selectedSeatsJson = await _js.InvokeAsync<string?>("sessionStorage.getItem", SelectedSeatsKey);
                if (!string.IsNullOrWhiteSpace(selectedSeatsJson))
                {
                    var seats = JsonSerializer.Deserialize<List<SelectedSeatClientDto>>(selectedSeatsJson, JsonOptions);
                    SelectedSeats = seats ?? new List<SelectedSeatClientDto>();
                }
                else
                {
                    SelectedSeats = new List<SelectedSeatClientDto>();
                }

                // 🔥 cart
                var cartJson = await _js.InvokeAsync<string?>("sessionStorage.getItem", CartKey);
                if (!string.IsNullOrWhiteSpace(cartJson))
                {
                    var cartSeats = JsonSerializer.Deserialize<List<SelectedSeatClientDto>>(cartJson, JsonOptions);
                    Cart = cartSeats ?? new List<SelectedSeatClientDto>();
                }
                else
                {
                    Cart = new List<SelectedSeatClientDto>();
                }
            }
            catch
            {
                CachedFilms = new List<FilmDto>();
                Cart = new List<SelectedSeatClientDto>();
            }

            _initialized = true;
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
            CachedFilms = new List<FilmDto>();
            Cart = new List<SelectedSeatClientDto>();
            SelectedSeats = new List<SelectedSeatClientDto>();
            SelectedShowtime = null;

            await _js.InvokeVoidAsync("sessionStorage.removeItem", StorageKey);
            await _js.InvokeVoidAsync("sessionStorage.removeItem", FilmCacheKey);
            await _js.InvokeVoidAsync("sessionStorage.removeItem", SelectedShowtimeKey);
            await _js.InvokeVoidAsync("sessionStorage.removeItem", SelectedSeatsKey);
            await _js.InvokeVoidAsync("sessionStorage.removeItem", CartKey);   // 🔥
        }

        public async Task LogoutAsync(CancellationToken ct = default)
        {
            try
            {
                await _auth.LogoutAsync(ct);
            }
            catch
            {
            }
            finally
            {
                await ClearAsync();
            }
        }

        public async Task<string?> GetValidAccessTokenAsync(CancellationToken ct = default)
        {
            if (_session?.Token is null)
                return null;

            var now = DateTime.UtcNow;
            var expires = _session.Token.ExpiresAt;

            var secondsLeft = (expires - now).TotalSeconds;

            if (secondsLeft <= 0 || secondsLeft < 120)
            {
                await RefreshAsync(ct);
            }

            return _session?.Token.Token;
        }

        private async Task RefreshAsync(CancellationToken ct)
        {
            if (_refreshInProgress) return;

            if (_session?.RefreshToken == null)
            {
                await ClearAsync();
                return;
            }

            try
            {
                _refreshInProgress = true;

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

        public async Task StoreFilmsAsync(List<FilmDto> films)
        {
            CachedFilms = films ?? new List<FilmDto>();

            var json = JsonSerializer.Serialize(CachedFilms, JsonOptions);
            await _js.InvokeVoidAsync("sessionStorage.setItem", FilmCacheKey, json);
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

        public async Task ClearSelectedShowtimeAsync()
        {
            SelectedShowtime = null;
            await _js.InvokeVoidAsync("sessionStorage.removeItem", SelectedShowtimeKey);
        }

        public async Task SetSelectedSeatsAsync(List<SelectedSeatClientDto> seats)
        {
            SelectedSeats = seats ?? new List<SelectedSeatClientDto>();

            var json = JsonSerializer.Serialize(SelectedSeats, JsonOptions);
            await _js.InvokeVoidAsync("sessionStorage.setItem", SelectedSeatsKey, json);
        }

        public async Task ClearSelectedSeatsAsync()
        {
            SelectedSeats = new List<SelectedSeatClientDto>();
            await _js.InvokeVoidAsync("sessionStorage.removeItem", SelectedSeatsKey);
        }

        // 🔥 NEW: Cart helpers ------------------------------------------

        public async Task AddToCartAsync(IEnumerable<SelectedSeatClientDto> seats)
        {
            if (seats == null) return;

            // ensure initialized
            Cart ??= new List<SelectedSeatClientDto>();

            foreach (var seat in seats)
            {
                // avoid duplicates: same seat + same showtime
                if (!Cart.Any(c => c.SeatId == seat.SeatId && c.VorstellungId == seat.VorstellungId))
                {
                    Cart.Add(seat);
                }
            }

            var json = JsonSerializer.Serialize(Cart, JsonOptions);
            await _js.InvokeVoidAsync("sessionStorage.setItem", CartKey, json);
        }

        public async Task ClearCartAsync()
        {
            Cart = new List<SelectedSeatClientDto>();
            await _js.InvokeVoidAsync("sessionStorage.removeItem", CartKey);
        }
    }
}
