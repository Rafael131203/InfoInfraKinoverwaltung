using KinoAppShared.Enums;

namespace KinoAppShared.DTOs.Kinosaal
{
    /// <summary>
    /// Client-side representation of a selected seat for a showtime.
    /// </summary>
    public class SelectedSeatClientDto
    {
        /// <summary>
        /// Seat identifier.
        /// </summary>
        public long SeatId { get; set; }

        /// <summary>
        /// Associated showtime identifier.
        /// </summary>
        public long VorstellungId { get; set; }

        /// <summary>
        /// Row label (e.g. "Reihe 5").
        /// </summary>
        public string RowLabel { get; set; } = string.Empty;

        /// <summary>
        /// Seat number within the row.
        /// </summary>
        public int SeatNumber { get; set; }

        /// <summary>
        /// Ticket price for the seat.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Seating category.
        /// </summary>
        public SitzreihenKategorie Category { get; set; }
    }
}
