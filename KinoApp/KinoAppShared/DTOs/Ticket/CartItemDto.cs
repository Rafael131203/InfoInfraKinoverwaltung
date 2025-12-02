using KinoAppShared.DTOs.Showtimes;

namespace KinoAppShared.DTOs.Ticket
{
    public class CartItemDto
    {
        public SelectedSeatClientDto Seat { get; set; } = default!;
        public int MovieId { get; set; }
        public string MovieTitle { get; set; } = "";
        public string? PosterUrl { get; set; }
        public ShowtimeDto Showtime { get; set; } = default!;
    }

}
