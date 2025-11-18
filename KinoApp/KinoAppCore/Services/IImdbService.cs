using KinoAppCore.Components;
using KinoAppShared.DTOs.Imdb;

namespace KinoAppCore.Services
{
    public interface IImdbService
    {
        Task<ImdbMovieDetails?> GetMovieByImdbIdAsync(
            string imdbId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ImdbMovieSearchResult>> SearchMoviesAsync(
            string query,
            CancellationToken cancellationToken = default);

        // NEW: advanced filter endpoint (movies only by default)
        Task<IReadOnlyList<ImdbMovieSearchResult>> ListMoviesAsync(
            ImdbListTitlesRequest request,
            CancellationToken cancellationToken = default);
    }
}
