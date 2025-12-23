namespace KinoAppCore.Components
{
    /// <summary>
    /// Represents detailed information about a movie retrieved from IMDb.
    /// </summary>
    public class ImdbMovieDetails
    {
        /// <summary>
        /// The IMDb identifier of the movie.
        /// </summary>
        public string ImdbId { get; set; } = string.Empty;

        /// <summary>
        /// The title of the movie.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The release year of the movie.
        /// </summary>
        public int? Year { get; set; }

        /// <summary>
        /// A short description or plot summary of the movie.
        /// </summary>
        public string? Plot { get; set; }

        /// <summary>
        /// The URL to the movie poster image.
        /// </summary>
        public string? PosterUrl { get; set; }

        /// <summary>
        /// The average IMDb rating of the movie.
        /// </summary>
        public double? Rating { get; set; }
    }
}
