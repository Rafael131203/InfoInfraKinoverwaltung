using System.Net.Http.Json;
using KinoAppShared.DTOs.Imdb;

namespace KinoAppWeb.Services
{
    /// <summary>
    /// Lightweight client for the KinoApp IMDb API endpoints exposed by the backend.
    /// </summary>
    /// <remarks>
    /// This client focuses on application-level endpoints (e.g. local DB film list) rather than direct IMDb calls.
    /// </remarks>
    public class ImdbApiClient
    {
        private readonly HttpClient _http;

        /// <summary>
        /// Creates a new <see cref="ImdbApiClient"/>.
        /// </summary>
        /// <param name="http">HTTP client configured with the backend base address.</param>
        public ImdbApiClient(HttpClient http)
        {
            _http = http;
        }

        /// <summary>
        /// Returns films stored in the local database.
        /// </summary>
        /// <remarks>
        /// Used by the UI to populate film lists without querying the external IMDb API.
        /// </remarks>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task<List<FilmDto>> GetLocalFilmsAsync(CancellationToken cancellationToken = default)
        {
            var films = await _http.GetFromJsonAsync<List<FilmDto>>("api/imdb/local", cancellationToken);
            return films ?? new List<FilmDto>();
        }
    }
}
