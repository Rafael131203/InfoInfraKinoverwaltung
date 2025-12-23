using KinoAppCore.Components;
using KinoAppDB.Entities;
using KinoAppShared.DTOs.Imdb;

namespace KinoAppCore.Services
{
    /// <summary>
    /// Provides access to IMDb title data and synchronizes selected titles with the local film catalog.
    /// </summary>
    /// <remarks>
    /// Implementations typically call an external IMDb-compatible API for search and metadata, and may optionally
    /// import or refresh records in the local database.
    /// </remarks>
    public interface IImdbService
    {
        /// <summary>
        /// Retrieves movie details for a specific IMDb title ID.
        /// </summary>
        /// <param name="imdbId">The IMDb title ID (e.g., <c>tt1234567</c>).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The movie details if found; otherwise <c>null</c>.</returns>
        Task<ImdbMovieDetails?> GetMovieByImdbIdAsync(string imdbId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches for titles matching the specified query.
        /// </summary>
        /// <param name="query">Free text query (title keywords).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A read-only list of matching titles (may be empty).</returns>
        Task<IReadOnlyList<ImdbMovieSearchResult>> SearchMoviesAsync(string query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists titles using the specified filter request.
        /// </summary>
        /// <param name="request">Filter request for listing titles.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A read-only list of titles (may be empty).</returns>
        Task<IReadOnlyList<ImdbMovieSearchResult>> ListMoviesAsync(ImdbListTitlesRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists titles using the specified filter request and imports a subset into the local catalog.
        /// </summary>
        /// <param name="request">Filter request for listing titles.</param>
        /// <param name="importCount">Maximum number of titles to import into the local database.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A read-only list of titles (may be empty).</returns>
        Task<IReadOnlyList<ImdbMovieSearchResult>> ListMoviesAsync(ImdbListTitlesRequest request, int importCount, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves certificate (age rating) information for the specified IMDb title.
        /// </summary>
        /// <param name="imdbId">The IMDb title ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The certificate response if available; otherwise <c>null</c>.</returns>
        Task<ListTitleCertificatesApiResponseDto?> GetTitleCertificatesAsync(string imdbId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Refreshes all locally stored films from the external title source and updates local fields.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task RefreshAllFilmsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns all films currently stored in the local catalog.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A read-only list of film DTOs.</returns>
        Task<IReadOnlyList<FilmDto>> GetAllLocalFilmsAsync(CancellationToken ct = default);

        /// <summary>
        /// Adds a film to the local catalog.
        /// </summary>
        /// <param name="movie">Film entity to insert.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The persisted entity (or the input entity if it already exists).</returns>
        Task<FilmEntity> AddMovieAsync(FilmEntity movie, CancellationToken ct = default);

        /// <summary>
        /// Deletes a film from the local catalog by its ID.
        /// </summary>
        /// <param name="movieId">The film identifier (typically the IMDb title ID).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns><c>true</c> if a film was deleted; otherwise <c>false</c>.</returns>
        Task<bool> DeleteMovieAsync(string movieId, CancellationToken ct = default);
    }
}
