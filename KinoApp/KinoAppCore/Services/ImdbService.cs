using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Linq;                         // <== needed for LINQ
using AutoMapper;
using KinoAppCore.Components;
using KinoAppCore.Entities;
using KinoAppDB.Entities;
using KinoAppDB.Repository;
using KinoAppShared.DTOs.Imdb;

namespace KinoAppCore.Services
{
    public class ImdbService : IImdbService
    {
        private readonly HttpClient _httpClient;
        private readonly IMapper _mapper;
        private readonly IFilmRepository _filmRepository;

        // how many movies to import when no count is specified
        private const int DefaultImportCount = 50;

        public ImdbService(HttpClient httpClient, IMapper mapper, IFilmRepository filmRepository)
        {
            _httpClient = httpClient;
            _mapper = mapper;
            _filmRepository = filmRepository;
        }

        // --- search/titles?query=... (no DB import here) ---
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

        // --- list/filter movies via GET /titles (imports DefaultImportCount movies) ---
        public Task<IReadOnlyList<ImdbMovieSearchResult>> ListMoviesAsync(ImdbListTitlesRequest request, CancellationToken cancellationToken = default)
        {
            // always import some movies by default
            return ListMoviesInternalAsync(request, DefaultImportCount, cancellationToken);
        }

        // --- list/filter movies via GET /titles AND import N movies into DB ---
        public Task<IReadOnlyList<ImdbMovieSearchResult>> ListMoviesAsync(ImdbListTitlesRequest request, int importCount, CancellationToken cancellationToken = default)
        {
            return ListMoviesInternalAsync(request, importCount, cancellationToken);
        }

        // shared implementation
        private async Task<IReadOnlyList<ImdbMovieSearchResult>> ListMoviesInternalAsync(ImdbListTitlesRequest request, int importCount, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // sanitize importCount
            if (importCount < 0) importCount = 0;

            // Force movie type as default
            if (request.Types == null || request.Types.Count == 0)
                request.Types = new List<string> { "MOVIE" };

            // IGNORE incoming pageToken
            request.PageToken = null;

            // FORCE limit=20 on first page only
            const int maxLimit = 50;
            var queryString = BuildTitlesQueryString(request);

            // Add limit if not present
            if (!queryString.Contains("limit="))
                queryString += (queryString.Contains("?") ? "&" : "?") + "limit=" + maxLimit;

            // --- Fetch FIRST PAGE ONLY ---
            var response = await _httpClient.GetAsync($"/titles{queryString}", cancellationToken);
            if (!response.IsSuccessStatusCode)
                return Array.Empty<ImdbMovieSearchResult>();

            var listResponse = await response.Content.ReadFromJsonAsync<SearchTitlesApiResponseDto>(
                cancellationToken);

            if (listResponse?.Titles == null || listResponse.Titles.Count == 0)
                return Array.Empty<ImdbMovieSearchResult>();

            // Convert API → search result (max 20 movies)
            var results = listResponse.Titles
                .Take(maxLimit)
                .Select(MapTitleToSearchResult)
                .ToList();

            // --- Import logic (ALWAYS possible, importCount controls HOW MANY) ---
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

        // helper: convert ImdbTitleApiDto -> ImdbMovieSearchResult
        private static ImdbMovieSearchResult MapTitleToSearchResult(ImdbTitleApiDto t) =>
            new ImdbMovieSearchResult
            {
                // this Id is the IMDb id (e.g. "tt30144839")
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

        // --- import one movie into DB using AutoMapper + certificates (age rating) ---
        private async Task ImportFilmAsync(ImdbMovieSearchResult movie, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(movie.Id))
                return;

            // avoid duplicates by IMDb id
            bool exists = await _filmRepository.AnyAsync(
                f => f.Id == movie.Id,
                cancellationToken);

            if (exists)
                return;

            // get certificates for FSK
            var certsResponse = await GetTitleCertificatesAsync(movie.Id, cancellationToken);
            var deCertificate = certsResponse?.Certificates?.FirstOrDefault(c => string.Equals(c.Country?.Code, "DE", StringComparison.OrdinalIgnoreCase)) ?? certsResponse?.Certificates?.FirstOrDefault();

            var fsk = MapCertificateToFsk(deCertificate);

            // direct mapping IMDb -> FilmEntity
            var entity = _mapper.Map<FilmEntity>(movie);
            entity.Fsk = fsk;

            await _filmRepository.AddAsync(entity, cancellationToken);
            await _filmRepository.SaveAsync(cancellationToken);
        }


        // simple mapping from IMDb rating string -> FSK (age)
        private static int? MapCertificateToFsk(ImdbCertificateApiDto? cert)
        {
            if (cert?.Rating == null)
                return null;

            var digits = new string(cert.Rating.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out var age))
                return age;

            return null;
        }

        // helper to build query string in "multi" format for arrays
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

        // existing detail endpoint
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

        public async Task<ListTitleCertificatesApiResponseDto?> GetTitleCertificatesAsync(string imdbId, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"/titles/{imdbId}/certificates", cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<ListTitleCertificatesApiResponseDto>(
                cancellationToken: cancellationToken);
        }

        // helper: raw /titles/{id} call that returns the API DTO
        private async Task<ImdbTitleApiDto?> GetTitleApiAsync(string imdbId, CancellationToken ct)
        {
            var response = await _httpClient.GetAsync($"/titles/{imdbId}", ct);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<ImdbTitleApiDto>(
                cancellationToken: ct);
        }

        // PUBLIC: refresh all films currently in DB
        public async Task RefreshAllFilmsAsync(CancellationToken ct = default)
        {
            // get all films currently stored in DB
            var films = await _filmRepository.GetAllAsync(ct);

            foreach (var filmEntity in films)
            {
                if (string.IsNullOrWhiteSpace(filmEntity.Id))
                    continue;

                // 1) get latest movie info from IMDb
                var apiDto = await GetTitleApiAsync(filmEntity.Id, ct);
                if (apiDto == null)
                    continue;

                // 2) map API dto -> FilmEntity (using AutoMapper)
                var refreshed = _mapper.Map<FilmEntity>(apiDto);

                // copy fields you want to keep up-to-date
                filmEntity.Titel = refreshed.Titel;
                filmEntity.Beschreibung = refreshed.Beschreibung;
                filmEntity.Dauer = refreshed.Dauer;
                filmEntity.Genre = refreshed.Genre;
                filmEntity.ImageURL = refreshed.ImageURL;

                // 3) refresh FSK via certificates
                var certsResponse = await GetTitleCertificatesAsync(filmEntity.Id, ct);
                var deCertificate = certsResponse?.Certificates?
                    .FirstOrDefault(c => string.Equals(c.Country?.Code, "DE", StringComparison.OrdinalIgnoreCase))
                    ?? certsResponse?.Certificates?.FirstOrDefault();

                filmEntity.Fsk = MapCertificateToFsk(deCertificate);

                await _filmRepository.UpdateAsync(filmEntity, ct);
            }

            // one SaveChanges at the end
            await _filmRepository.SaveAsync(ct);
        }

        public async Task<IReadOnlyList<FilmDto>> GetAllLocalFilmsAsync(CancellationToken ct = default)
        {
            var films = await _filmRepository.GetAllAsync(ct);
            var dto = films.Select(f => _mapper.Map<FilmDto>(f)).ToList();
            return dto;
        }

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
