using KinoAppShared.DTOs.Kinosaal;
using KinoAppShared.DTOs.Showtimes;

namespace KinoAppShared.DTOs.Ticket
{
    /// <summary>
    /// Represents a single item inside the client-side shopping cart.
    /// </summary>
    public class CartItemDto
    {
        /// <summary>
        /// Selected seat information.
        /// </summary>
        public SelectedSeatClientDto Seat { get; set; } = default!;

        /// <summary>
        /// Identifier of the associated movie.
        /// </summary>
        public int MovieId { get; set; }

        /// <summary>
        /// Movie title.
        /// </summary>
        public string MovieTitle { get; set; } = string.Empty;

        /// <summary>
        /// Optional poster image URL.
        /// </summary>
        public string? PosterUrl { get; set; }

        /// <summary>
        /// Selected showtime.
        /// </summary>
        public ShowtimeDto Showtime { get; set; } = default!;
    }
}
