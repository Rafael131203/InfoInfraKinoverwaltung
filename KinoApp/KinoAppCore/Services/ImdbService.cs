using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using KinoAppCore.Components;
using KinoAppShared.DTOs.Imdb;

namespace KinoAppCore.Services
{
    public class ImdbService : IImdbService
    {
        private readonly HttpClient _httpClient;

        public ImdbService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // --- existing method you already have ---
        public async Task<IReadOnlyList<ImdbMovieSearchResult>> SearchMoviesAsync(
            string query,
            CancellationToken cancellationToken = default)
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

            return searchResponse.Titles.Select(t => new ImdbMovieSearchResult
            {
                ImdbId = t.Id,
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
            })
            .ToList();
        }

        // --- new: list/filter movies via GET /titles ---
        public async Task<IReadOnlyList<ImdbMovieSearchResult>> ListMoviesAsync(
            ImdbListTitlesRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // ensure movies-only default
            if (request.Types == null || request.Types.Count == 0)
            {
                request.Types = new List<string> { "MOVIE" };
            }

            var queryString = BuildTitlesQueryString(request);

            var response = await _httpClient.GetAsync($"/titles{queryString}", cancellationToken);
            if (!response.IsSuccessStatusCode)
                return Array.Empty<ImdbMovieSearchResult>();

            // The list-titles response has the same "titles" array shape as search,
            // so we can reuse SearchTitlesApiResponseDto.
            var listResponse = await response.Content.ReadFromJsonAsync<SearchTitlesApiResponseDto>(
                cancellationToken: cancellationToken);

            if (listResponse?.Titles == null || listResponse.Titles.Count == 0)
                return Array.Empty<ImdbMovieSearchResult>();

            return listResponse.Titles.Select(t => new ImdbMovieSearchResult
            {
                ImdbId = t.Id,
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
            })
            .ToList();

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

        // you also still have GetMovieByImdbIdAsync here…
        public async Task<ImdbMovieDetails?> GetMovieByImdbIdAsync(
            string imdbId,
            CancellationToken cancellationToken = default)
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
    }
}
