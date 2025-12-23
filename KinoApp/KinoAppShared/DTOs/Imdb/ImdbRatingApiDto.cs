namespace KinoAppShared.DTOs.Imdb
{
    /// <summary>
    /// Rating information returned by the IMDb API.
    /// </summary>
    public class ImdbRatingApiDto
    {
        /// <summary>
        /// Aggregated rating value (e.g. IMDb score).
        /// </summary>
        public double? AggregateRating { get; set; }

        /// <summary>
        /// Total number of votes contributing to the rating.
        /// </summary>
        public int? VoteCount { get; set; }
    }
}
