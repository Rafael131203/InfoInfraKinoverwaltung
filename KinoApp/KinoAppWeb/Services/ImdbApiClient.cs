using KinoAppCore.Components;
using KinoAppShared.DTOs.Imdb;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;

namespace KinoAppWeb.Services
{
    public class ImdbApiClient
    {
        private readonly HttpClient _http;

        public ImdbApiClient(HttpClient http)
        {
            _http = http;
        }

        // existing method (kept if still used somewhere)
        public async Task<List<ImdbMovieSearchResult>> SearchAsync(
            string query,
            CancellationToken cancellationToken = default)
        {
            var results = await _http.GetFromJsonAsync<List<ImdbMovieSearchResult>>(
                $"api/imdb/search?query={Uri.EscapeDataString(query)}",
                cancellationToken);

            return results ?? new List<ImdbMovieSearchResult>();
        }

        // filtered search using IMDb API (remote) – keep if you still need it
        public async Task<List<ImdbMovieSearchResult>> SearchFilteredAsync(
            string? genre = null,
            string? languageCode = null,
            string? sortBy = null,
            string? sortOrder = "DESC",
            CancellationToken cancellationToken = default)
        {
            var sb = new StringBuilder("api/imdb/search");

            var qp = new List<string>();

            if (!string.IsNullOrWhiteSpace(genre) && genre != "all")
            {
                qp.Add($"genres={Uri.EscapeDataString(genre)}");
            }

            if (!string.IsNullOrWhiteSpace(languageCode) && languageCode != "all")
            {
                qp.Add($"languageCodes={Uri.EscapeDataString(languageCode)}");
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                qp.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
            }

            if (!string.IsNullOrWhiteSpace(sortOrder))
            {
                qp.Add($"sortOrder={Uri.EscapeDataString(sortOrder)}");
            }

            if (qp.Count > 0)
            {
                sb.Append('?');
                sb.Append(string.Join("&", qp));
            }

            var url = sb.ToString();

            var results = await _http.GetFromJsonAsync<List<ImdbMovieSearchResult>>(
                url,
                cancellationToken);

            return results ?? new List<ImdbMovieSearchResult>();
        }

        /// <summary>
        /// DB-backed films endpoint – used together with UserSession caching.
        /// </summary>
        public async Task<List<FilmDto>> GetLocalFilmsAsync(CancellationToken cancellationToken = default)
        {
            var films = await _http.GetFromJsonAsync<List<FilmDto>>(
                "api/imdb/local",
                cancellationToken);

            return films ?? new List<FilmDto>();
        }
    }
}
