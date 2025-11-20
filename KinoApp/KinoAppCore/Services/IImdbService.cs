using KinoAppCore.Components;
using KinoAppShared.DTOs.Imdb;
using System.Threading;

namespace KinoAppCore.Services
{
    public interface IImdbService
    {
        Task<ImdbMovieDetails?> GetMovieByImdbIdAsync(string imdbId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ImdbMovieSearchResult>> SearchMoviesAsync(string query, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ImdbMovieSearchResult>> ListMoviesAsync(ImdbListTitlesRequest request, CancellationToken cancellationToken = default);
        public Task<IReadOnlyList<ImdbMovieSearchResult>> ListMoviesAsync(ImdbListTitlesRequest request, int importCount, CancellationToken cancellationToken = default);

        /// <summary>
        /// Calls https://api.imdbapi.dev/titles/{imdbId}/certificates
        /// and returns the raw certificate response.
        /// </summary>
        Task<ListTitleCertificatesApiResponseDto?> GetTitleCertificatesAsync(string imdbId, CancellationToken cancellationToken = default);

        Task RefreshAllFilmsAsync(CancellationToken cancellationToken = default);

    }
}
