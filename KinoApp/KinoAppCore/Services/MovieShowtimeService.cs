using System.Net.Http;
using System.Net.Http.Json;
using KinoAppShared.DTOs.Showtimes;

namespace KinoAppCore.Services
{
    /// <summary>
    /// Blazor WebAssembly client implementation of IMovieShowtimeService.
    /// Calls the backend API (api/showtimes) and returns DTOs.
    /// </summary>
    public class MovieShowtimeService : IMovieShowtimeService
    {
        private readonly HttpClient _http;
        private const string BaseUrl = "api/showtimes";

        public MovieShowtimeService(HttpClient http)
        {
            _http = http;
        }

        public Task<IReadOnlyList<MovieShowtimeDto>> GetTodayAsync()
        {
            // Let the server decide “today” or just reuse GetByDateAsync(DateTime.Today)
            return GetByDateAsync(DateTime.Today);
        }

        public async Task<IReadOnlyList<MovieShowtimeDto>> GetByDateAsync(DateTime date)
        {
            // Use yyyy-MM-dd so it’s easy to parse server-side
            var formatted = date.ToString("yyyy-MM-dd");

            // GET api/showtimes?date=2025-02-03
            var url = $"{BaseUrl}?date={Uri.EscapeDataString(formatted)}";

            var list = await _http.GetFromJsonAsync<List<MovieShowtimeDto>>(url);

            return (IReadOnlyList<MovieShowtimeDto>)(list ?? new List<MovieShowtimeDto>());
        }
    }
}
