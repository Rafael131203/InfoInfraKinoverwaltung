namespace KinoAppShared.DTOs.Ticket
{
    /// <summary>
    /// Request DTO for reserving one or more tickets.
    /// </summary>
    public class ReserveTicketDTO
    {
        /// <summary>
        /// Showtime identifier.
        /// </summary>
        public long VorstellungId { get; set; }

        /// <summary>
        /// Identifiers of the seats to reserve.
        /// </summary>
        public List<long> SitzplatzIds { get; set; } = new();
    }
}
