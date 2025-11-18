namespace KinoAppShared.DTOs.Imdb
{
    /// <summary>
    /// Filter options for listing titles (mapped to GET /titles on imdbapi.dev).
    /// All properties are optional; Types defaults to MOVIE only.
    /// </summary>
    public class ImdbListTitlesRequest
    {
        // array<string> types
        // Allowed: MOVIE, TV_SERIES, TV_MINI_SERIES, TV_SPECIAL, TV_MOVIE, SHORT, VIDEO, VIDEO_GAME
        public List<string>? Types { get; set; } = new() { "MOVIE" };

        // array<string> genres
        public List<string>? Genres { get; set; }

        // array<string> countryCodes (ISO 3166-1 alpha-2)
        public List<string>? CountryCodes { get; set; }

        // array<string> languageCodes (ISO 639-1 or 639-2)
        public List<string>? LanguageCodes { get; set; }

        // array<string> nameIds
        public List<string>? NameIds { get; set; }

        // array<string> interestIds
        public List<string>? InterestIds { get; set; }

        // int32 startYear / endYear
        public int? StartYear { get; set; } = 2025;
        public int? EndYear { get; set; }

        // int32 min/maxVoteCount
        public int? MinVoteCount { get; set; } = 10000;
        public int? MaxVoteCount { get; set; }

        // float min/maxAggregateRating (0.0 – 10.0)
        public float? MinAggregateRating { get; set; } = 8.0f;
        public float? MaxAggregateRating { get; set; }

        // string sortBy (SORT_BY_POPULARITY, SORT_BY_RELEASE_DATE, SORT_BY_USER_RATING, etc.)
        public string? SortBy { get; set; }

        // string sortOrder (ASC or DESC)
        public string? SortOrder { get; set; } = "ASC";

        // string pageToken (pagination cursor)
        public string? PageToken { get; set; }
    }
}
