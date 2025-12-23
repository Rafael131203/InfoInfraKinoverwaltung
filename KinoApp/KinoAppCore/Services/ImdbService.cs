using System.Net.Http;
using System.Net.Http.Json;
using System.Linq;
using AutoMapper;
using KinoAppCore.Components;
using KinoAppDB.Entities;
using KinoAppDB.Repository;
using KinoAppShared.DTOs.Imdb;

namespace KinoAppCore.Services
{
    /// <summary>
    /// IMDb API client and local film catalog synchronizer.
    /// </summary>
    /// <remarks>
    /// This service queries the configured IMDb-compatible API for title search and listing, and can optionally
    /// import or refresh titles in the local database. It relies on AutoMapper for API-to-entity mapping and uses
    /// certificates data to derive an FSK value where available.
    /// </remarks>
    public class ImdbService : IImdbService
    {
        private readonly HttpClient _httpClient;
        private readonly IMapper _mapper;
        private readonly IFilmRepository _filmRepository;

        /// <summary>
        /// Default number of titles imported when a caller does not provide an explicit import count.
        /// </summary>
        private const int DefaultImportCount = 50;

        /// <summary>
        /// Creates a new <see cref="ImdbService"/>.
        /// </summary>
        /// <param name="httpClient">HTTP client configured for the IMDb API base address.</param>
        /// <param name="mapper">Mapper used to convert API DTOs to local entities and DTOs.</param>
        /// <param name="filmRepository">Repository used to persist and query local films.</param>
        public ImdbService(HttpClient httpClient, IMapper mapper, IFilmRepository filmRepository)
        {
            _httpClient = httpClient;
            _mapper = mapper;
            _filmRepository = filmRepository;
        }

        /// <summary>
        /// Searches titles via <c>/search/titles</c> without importing results into the local catalog.
        /// </summary>
        /// <param name="query">Search query text.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A read-only list of matching titles (may be empty).</returns>
        public async Task<IReadOnlyList<ImdbMovieSearchResult>> SearchMoviesAsync(string query, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync(
                $"/search/titles?query={Uri.EscapeDataString(query)}&limit=10",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
                return Array.Empty<ImdbMovieSearchResult>();

            var searchResponse = await response.Content.ReadFromJsonAsync<SearchTitlesApiResponseDto>(
                cancellationToken: cancellationToken);

            if (searchResponse?.Titles == null || searchResponse.Titles.Count == 0)
                return Array.Empty<ImdbMovieSearchResult>();

            return searchResponse.Titles
                .Select(MapTitleToSearchResult)
                .ToList();
        }

        /// <summary>
        /// Lists titles via <c>/titles</c> and imports a default number of results into the local catalog.
        /// </summary>
        /// <param name="request">Filter request for title listing.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A read-only list of titles returned by the API (may be empty).</returns>
        public Task<IReadOnlyList<ImdbMovieSearchResult>> ListMoviesAsync(ImdbListTitlesRequest request, CancellationToken cancellationToken = default)
        {
            return ListMoviesInternalAsync(request, DefaultImportCount, cancellationToken);
        }

        /// <summary>
        /// Lists titles via <c>/titles</c> and imports up to <paramref name="importCount"/> results into the local catalog.
        /// </summary>
        /// <param name="request">Filter request for title listing.</param>
        /// <param name="importCount">Maximum number of titles from the response to import.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A read-only list of titles returned by the API (may be empty).</returns>
        public Task<IReadOnlyList<ImdbMovieSearchResult>> ListMoviesAsync(ImdbListTitlesRequest request, int importCount, CancellationToken cancellationToken = default)
        {
            return ListMoviesInternalAsync(request, importCount, cancellationToken);
        }

        /// <summary>
        /// Shared implementation for title listing, normalization of request parameters, and optional import.
        /// </summary>
        /// <param name="request">Filter request for title listing.</param>
        /// <param name="importCount">Maximum number of titles to import.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A read-only list of titles returned by the API (may be empty).</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <c>null</c>.</exception>
        private async Task<IReadOnlyList<ImdbMovieSearchResult>> ListMoviesInternalAsync(ImdbListTitlesRequest request, int importCount, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (importCount < 0)
                importCount = 0;

            if (request.Types == null || request.Types.Count == 0)
                request.Types = new List<string> { "MOVIE" };

            request.PageToken = null;

            const int maxLimit = 50;
            var queryString = BuildTitlesQueryString(request);

            if (!queryString.Contains("limit="))
                queryString += (queryString.Contains("?") ? "&" : "?") + "limit=" + maxLimit;

            var response = await _httpClient.GetAsync($"/titles{queryString}", cancellationToken);
            if (!response.IsSuccessStatusCode)
                return Array.Empty<ImdbMovieSearchResult>();

            var listResponse = await response.Content.ReadFromJsonAsync<SearchTitlesApiResponseDto>(
                cancellationToken);

            if (listResponse?.Titles == null || listResponse.Titles.Count == 0)
                return Array.Empty<ImdbMovieSearchResult>();

            var results = listResponse.Titles
                .Take(maxLimit)
                .Select(MapTitleToSearchResult)
                .ToList();

            if (importCount > 0)
            {
                int toImport = Math.Min(importCount, maxLimit);

                foreach (var movie in results.Take(toImport))
                {
                    await ImportFilmAsync(movie, cancellationToken);
                }
            }

            return results;
        }

        /// <summary>
        /// Converts an API title DTO into the search/list result model used by the application.
        /// </summary>
        /// <param name="t">Title DTO returned by the API.</param>
        /// <returns>Mapped search result model.</returns>
        private static ImdbMovieSearchResult MapTitleToSearchResult(ImdbTitleApiDto t) =>
            new ImdbMovieSearchResult
            {
                Id = t.Id,
                Type = t.Type,
                Title = t.PrimaryTitle ?? string.Empty,
                OriginalTitle = t.OriginalTitle,
                Year = t.StartYear,
                RuntimeSeconds = t.RuntimeSeconds,
                Genres = t.Genres ?? new List<string>(),
                Rating = t.Rating?.AggregateRating,
                VoteCount = t.Rating?.VoteCount,
                Plot = t.Plot,
                PosterUrl = t.PrimaryImage?.Url,
                PosterWidth = t.PrimaryImage?.Width,
                PosterHeight = t.PrimaryImage?.Height
            };

        /// <summary>
        /// Imports a single title into the local catalog if it does not already exist.
        /// </summary>
        /// <param name="movie">The title to import.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <remarks>
        /// The import derives <c>Fsk</c> from certificate data when available. If no certificate is found,
        /// <c>Fsk</c> remains <c>null</c>.
        /// </remarks>
        private async Task ImportFilmAsync(ImdbMovieSearchResult movie, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(movie.Id))
                return;

            bool exists = await _filmRepository.AnyAsync(
                f => f.Id == movie.Id,
                cancellationToken);

            if (exists)
                return;

            var certsResponse = await GetTitleCertificatesAsync(movie.Id, cancellationToken);
            var deCertificate =
                certsResponse?.Certificates?.FirstOrDefault(c => string.Equals(c.Country?.Code, "DE", StringComparison.OrdinalIgnoreCase))
                ?? certsResponse?.Certificates?.FirstOrDefault();

            var fsk = MapCertificateToFsk(deCertificate);

            var entity = _mapper.Map<FilmEntity>(movie);
            entity.Fsk = fsk;

            await _filmRepository.AddAsync(entity, cancellationToken);
            await _filmRepository.SaveAsync(cancellationToken);
        }

        /// <summary>
        /// Extracts an FSK age value from a certificate rating string by parsing digits.
        /// </summary>
        /// <param name="cert">Certificate DTO.</param>
        /// <returns>The parsed age rating, or <c>null</c> if parsing is not possible.</returns>
        private static int? MapCertificateToFsk(ImdbCertificateApiDto? cert)
        {
            if (cert?.Rating == null)
                return null;

            var digits = new string(cert.Rating.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out var age))
                return age;

            return null;
        }

        /// <summary>
        /// Builds a query string for the <c>/titles</c> endpoint using a repeated-key format for list parameters.
        /// </summary>
        /// <param name="r">Request object containing filters and sort parameters.</param>
        /// <returns>A query string beginning with <c>?</c>, or an empty string if no parameters are set.</returns>
        private static string BuildTitlesQueryString(ImdbListTitlesRequest r)
        {
            var parts = new List<string>();

            void AddList(string name, IEnumerable<string>? values)
            {
                if (values == null) return;
                foreach (var v in values.Where(s => !string.IsNullOrWhiteSpace(s)))
                {
                    parts.Add($"{name}={Uri.EscapeDataString(v)}");
                }
            }

            void AddInt(string name, int? value)
            {
                if (value.HasValue)
                    parts.Add($"{name}={value.Value}");
            }

            void AddFloat(string name, float? value)
            {
                if (value.HasValue)
                    parts.Add($"{name}={value.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
            }

            void AddString(string name, string? value)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    parts.Add($"{name}={Uri.EscapeDataString(value)}");
            }

            AddList("types", r.Types);
            AddList("genres", r.Genres);
            AddList("countryCodes", r.CountryCodes);
            AddList("languageCodes", r.LanguageCodes);
            AddList("nameIds", r.NameIds);
            AddList("interestIds", r.InterestIds);

            AddInt("startYear", r.StartYear);
            AddInt("endYear", r.EndYear);
            AddInt("minVoteCount", r.MinVoteCount);
            AddInt("maxVoteCount", r.MaxVoteCount);

            AddFloat("minAggregateRating", r.MinAggregateRating);
            AddFloat("maxAggregateRating", r.MaxAggregateRating);

            AddString("sortBy", r.SortBy);
            AddString("sortOrder", r.SortOrder);
            AddString("pageToken", r.PageToken);

            if (parts.Count == 0)
                return string.Empty;

            return "?" + string.Join("&", parts);
        }

        /// <summary>
        /// Retrieves movie details via <c>/titles/{imdbId}</c>.
        /// </summary>
        /// <param name="imdbId">The IMDb title ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The movie details if available; otherwise <c>null</c>.</returns>
        public async Task<ImdbMovieDetails?> GetMovieByImdbIdAsync(string imdbId, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"/titles/{imdbId}", cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            var apiDto = await response.Content.ReadFromJsonAsync<ImdbTitleApiDto>(
                cancellationToken: cancellationToken);

            if (apiDto == null)
                return null;

            return new ImdbMovieDetails
            {
                ImdbId = apiDto.Id,
                Title = apiDto.PrimaryTitle ?? string.Empty,
                Year = apiDto.StartYear,
                Plot = apiDto.Plot,
                PosterUrl = apiDto.PrimaryImage?.Url,
                Rating = apiDto.Rating?.AggregateRating
            };
        }

        /// <summary>
        /// Retrieves title certificate information via <c>/titles/{imdbId}/certificates</c>.
        /// </summary>
        /// <param name="imdbId">The IMDb title ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The certificate response DTO if available; otherwise <c>null</c>.</returns>
        public async Task<ListTitleCertificatesApiResponseDto?> GetTitleCertificatesAsync(string imdbId, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"/titles/{imdbId}/certificates", cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<ListTitleCertificatesApiResponseDto>(
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Calls <c>/titles/{imdbId}</c> and returns the raw API DTO.
        /// </summary>
        /// <param name="imdbId">The IMDb title ID.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The raw title DTO if available; otherwise <c>null</c>.</returns>
        private async Task<ImdbTitleApiDto?> GetTitleApiAsync(string imdbId, CancellationToken ct)
        {
            var response = await _httpClient.GetAsync($"/titles/{imdbId}", ct);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<ImdbTitleApiDto>(
                cancellationToken: ct);
        }

        /// <summary>
        /// Refreshes all films currently stored in the local database using the latest external title data.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <remarks>
        /// This method updates the mutable film fields (title, plot, runtime, genre, image) and recalculates <c>Fsk</c>
        /// from certificate data. Changes are persisted in a single save operation at the end.
        /// </remarks>
        public async Task RefreshAllFilmsAsync(CancellationToken ct = default)
        {
            var films = await _filmRepository.GetAllAsync(ct);

            foreach (var filmEntity in films)
            {
                if (string.IsNullOrWhiteSpace(filmEntity.Id))
                    continue;

                var apiDto = await GetTitleApiAsync(filmEntity.Id, ct);
                if (apiDto == null)
                    continue;

                var refreshed = _mapper.Map<FilmEntity>(apiDto);

                filmEntity.Titel = refreshed.Titel;
                filmEntity.Beschreibung = refreshed.Beschreibung;
                filmEntity.Dauer = refreshed.Dauer;
                filmEntity.Genre = refreshed.Genre;
                filmEntity.ImageURL = refreshed.ImageURL;

                var certsResponse = await GetTitleCertificatesAsync(filmEntity.Id, ct);
                var deCertificate =
                    certsResponse?.Certificates?.FirstOrDefault(c => string.Equals(c.Country?.Code, "DE", StringComparison.OrdinalIgnoreCase))
                    ?? certsResponse?.Certificates?.FirstOrDefault();

                filmEntity.Fsk = MapCertificateToFsk(deCertificate);

                await _filmRepository.UpdateAsync(filmEntity, ct);
            }

            await _filmRepository.SaveAsync(ct);
        }

        /// <summary>
        /// Returns all films currently stored in the local database mapped to <see cref="FilmDto"/>.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A read-only list of film DTOs.</returns>
        public async Task<IReadOnlyList<FilmDto>> GetAllLocalFilmsAsync(CancellationToken ct = default)
        {
            var films = await _filmRepository.GetAllAsync(ct);
            var dto = films.Select(f => _mapper.Map<FilmDto>(f)).ToList();
            return dto;
        }

        /// <summary>
        /// Adds a film entity to the local database if it does not already exist.
        /// </summary>
        /// <param name="movie">Film entity to insert.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The inserted entity, or the provided entity if a record with the same ID already exists.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="movie"/> has an empty ID.</exception>
        public async Task<FilmEntity> AddMovieAsync(FilmEntity movie, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(movie.Id))
                throw new ArgumentException("Film Id cannot be empty.");

            bool exists = await _filmRepository.AnyAsync(f => f.Id == movie.Id, ct);
            if (exists)
                return movie;

            await _filmRepository.AddAsync(movie, ct);
            await _filmRepository.SaveAsync(ct);
            return movie;
        }

        /// <summary>
        /// Deletes a film from the local database by its identifier.
        /// </summary>
        /// <param name="movieId">Film identifier (typically the IMDb title ID).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns><c>true</c> if a film was deleted; otherwise <c>false</c>.</returns>
        public async Task<bool> DeleteMovieAsync(string movieId, CancellationToken ct = default)
        {
            var film = await _filmRepository.GetByIdAsync(movieId, ct);
            if (film == null)
                return false;

            await _filmRepository.DeleteAsync(film, ct);
            await _filmRepository.SaveAsync(ct);
            return true;
        }
    }
}
