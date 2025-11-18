using System.Net.Http;
using System.Net.Http.Json;
using KinoAppCore.Components;

namespace KinoAppWeb.Services
{
    public class ImdbApiClient
    {
        private readonly HttpClient _http;

        public ImdbApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ImdbMovieSearchResult>> SearchAsync(
            string query,
            CancellationToken cancellationToken = default)
        {
            var results = await _http.GetFromJsonAsync<List<ImdbMovieSearchResult>>(
                $"api/imdb/search?query={Uri.EscapeDataString(query)}",
                cancellationToken);

            return results ?? new List<ImdbMovieSearchResult>();
        }

        public Task<ImdbMovieDetails?> GetByIdAsync(
            string imdbId,
            CancellationToken cancellationToken = default)
        {
            return _http.GetFromJsonAsync<ImdbMovieDetails?>(
                $"api/imdb/{imdbId}",
                cancellationToken);
        }
    }
}
