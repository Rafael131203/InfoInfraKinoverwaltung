namespace KinoAppShared.DTOs.Showtimes
{
    public class SelectedShowtimeDto
    {
        public int MovieId { get; set; }
        public string? MovieTitle { get; set; }
        public string? PosterUrl { get; set; }

        public ShowtimeDto? Showtime { get; set; }
    }
}
