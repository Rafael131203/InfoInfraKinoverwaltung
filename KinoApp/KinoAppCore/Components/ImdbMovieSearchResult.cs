namespace KinoAppCore.Components
{
    /// <summary>
    /// Represents a single movie entry returned from an IMDb search query.
    /// </summary>
    public class ImdbMovieSearchResult
    {
        /// <summary>
        /// The IMDb identifier of the movie.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The type of result (e.g., movie, series, episode).
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// The display title of the movie.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The original title of the movie, if different from the display title.
        /// </summary>
        public string? OriginalTitle { get; set; }

        /// <summary>
        /// The release year of the movie.
        /// </summary>
        public int? Year { get; set; }

        /// <summary>
        /// The runtime of the movie in seconds.
        /// </summary>
        public int? RuntimeSeconds { get; set; }

        /// <summary>
        /// The list of genres associated with the movie.
        /// </summary>
        public List<string> Genres { get; set; } = new();

        /// <summary>
        /// The average IMDb rating of the movie.
        /// </summary>
        public double? Rating { get; set; }

        /// <summary>
        /// The number of votes contributing to the rating.
        /// </summary>
        public int? VoteCount { get; set; }

        /// <summary>
        /// A short description or plot summary of the movie.
        /// </summary>
        public string? Plot { get; set; }

        /// <summary>
        /// The URL to the movie poster image.
        /// </summary>
        public string? PosterUrl { get; set; }

        /// <summary>
        /// The width of the poster image in pixels.
        /// </summary>
        public int? PosterWidth { get; set; }

        /// <summary>
        /// The height of the poster image in pixels.
        /// </summary>
        public int? PosterHeight { get; set; }
    }
}
