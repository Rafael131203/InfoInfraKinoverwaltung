namespace KinoAppShared.DTOs.Showtimes
{
    /// <summary>
    /// DTO representing a selected showtime together with its movie context.
    /// </summary>
    public class SelectedShowtimeDto
    {
        /// <summary>
        /// Movie identifier.
        /// </summary>
        public int MovieId { get; set; }

        /// <summary>
        /// Movie title.
        /// </summary>
        public string? MovieTitle { get; set; }

        /// <summary>
        /// Poster image URL.
        /// </summary>
        public string? PosterUrl { get; set; }

        /// <summary>
        /// Selected showtime details.
        /// </summary>
        public ShowtimeDto? Showtime { get; set; }
    }
}
