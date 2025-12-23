namespace KinoAppShared.DTOs.Showtimes
{
    /// <summary>
    /// DTO representing a movie and its available showtimes for a given day.
    /// </summary>
    public class MovieShowtimeDto
    {
        /// <summary>
        /// Movie identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Movie title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Optional tagline or short subtitle.
        /// </summary>
        public string Tagline { get; set; } = string.Empty;

        /// <summary>
        /// Movie description or synopsis.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Poster image URL.
        /// </summary>
        public string PosterUrl { get; set; } = string.Empty;

        /// <summary>
        /// Movie duration in minutes.
        /// </summary>
        public int DurationMinutes { get; set; }

        /// <summary>
        /// Age rating label (e.g. "FSK 12").
        /// </summary>
        public string AgeRating { get; set; } = "FSK";

        /// <summary>
        /// Genre list formatted for display.
        /// </summary>
        public string Genres { get; set; } = string.Empty;

        /// <summary>
        /// Available showtimes for the movie.
        /// </summary>
        public IReadOnlyList<ShowtimeDto> Showtimes { get; set; } = Array.Empty<ShowtimeDto>();
    }
}
