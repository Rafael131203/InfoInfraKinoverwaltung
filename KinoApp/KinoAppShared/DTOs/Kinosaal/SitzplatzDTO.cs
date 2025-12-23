using KinoAppShared.Enums;

namespace KinoAppShared.DTOs.Kinosaal
{
    /// <summary>
    /// DTO representing a single seat.
    /// </summary>
    public class SitzplatzDTO
    {
        /// <summary>
        /// Seat identifier.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Seat number within the row.
        /// </summary>
        public int Nummer { get; set; }

        /// <summary>
        /// Ticket price for this seat.
        /// </summary>
        public decimal Preis { get; set; }

        /// <summary>
        /// Indicates whether the seat is currently booked.
        /// </summary>
        public bool Gebucht { get; set; }

        /// <summary>
        /// Current ticket status.
        /// </summary>
        public TicketStatus Status { get; set; }
    }
}
