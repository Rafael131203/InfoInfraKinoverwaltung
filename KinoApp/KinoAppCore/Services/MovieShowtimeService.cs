using System.Net.Http;
using System.Net.Http.Json;
using KinoAppShared.DTOs.Showtimes;

namespace KinoAppCore.Services
{
    /// <summary>
    /// HTTP-based client implementation of <see cref="IMovieShowtimeService"/> that calls the backend showtimes API.
    /// </summary>
    /// <remarks>
    /// This implementation is intended for UI clients (e.g., Blazor WebAssembly) and delegates date handling to the server
    /// via the query parameter format <c>yyyy-MM-dd</c>.
    /// </remarks>
    public class MovieShowtimeService : IMovieShowtimeService
    {
        private readonly HttpClient _http;

        /// <summary>
        /// Base relative URL for the showtimes endpoint.
        /// </summary>
        private const string BaseUrl = "api/showtimes";

        /// <summary>
        /// Creates a new <see cref="MovieShowtimeService"/>.
        /// </summary>
        /// <param name="http">HTTP client configured to reach the backend API.</param>
        public MovieShowtimeService(HttpClient http)
        {
            _http = http;
        }

        /// <inheritdoc />
        public Task<IReadOnlyList<MovieShowtimeDto>> GetTodayAsync()
        {
            return GetByDateAsync(DateTime.Today);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<MovieShowtimeDto>> GetByDateAsync(DateTime date)
        {
            var formatted = date.ToString("yyyy-MM-dd");
            var url = $"{BaseUrl}?date={Uri.EscapeDataString(formatted)}";

            var list = await _http.GetFromJsonAsync<List<MovieShowtimeDto>>(url);
            return (IReadOnlyList<MovieShowtimeDto>)(list ?? new List<MovieShowtimeDto>());
        }
    }
}
