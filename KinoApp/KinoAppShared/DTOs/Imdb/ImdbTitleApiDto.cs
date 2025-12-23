namespace KinoAppShared.DTOs.Imdb
{
    /// <summary>
    /// Detailed title information returned by the IMDb API.
    /// </summary>
    public class ImdbTitleApiDto
    {
        /// <summary>
        /// IMDb identifier (e.g. "tt1234567").
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Title type (movie, tvSeries, short, etc.).
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Primary display title.
        /// </summary>
        public string? PrimaryTitle { get; set; }

        /// <summary>
        /// Original title in the source language.
        /// </summary>
        public string? OriginalTitle { get; set; }

        /// <summary>
        /// Primary poster or image.
        /// </summary>
        public ImdbImageApiDto? PrimaryImage { get; set; }

        /// <summary>
        /// Release year.
        /// </summary>
        public int? StartYear { get; set; }

        /// <summary>
        /// Runtime in seconds.
        /// </summary>
        public int? RuntimeSeconds { get; set; }

        /// <summary>
        /// List of genres associated with the title.
        /// </summary>
        public List<string>? Genres { get; set; }

        /// <summary>
        /// Rating metadata.
        /// </summary>
        public ImdbRatingApiDto? Rating { get; set; }

        /// <summary>
        /// Plot or short description.
        /// </summary>
        public string? Plot { get; set; }
    }
}
