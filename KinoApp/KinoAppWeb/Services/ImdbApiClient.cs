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
