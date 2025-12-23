namespace KinoAppShared.DTOs.Ticket
{
    /// <summary>
    /// Request DTO for updating the status of a ticket.
    /// </summary>
    public class UpdateTicketStatusDTO
    {
        /// <summary>
        /// Ticket identifier.
        /// </summary>
        public long TicketId { get; set; }

        /// <summary>
        /// Target status as a string (e.g. "free", "reserved", "booked").
        /// </summary>
        public string Status { get; set; } = default!;

        /// <summary>
        /// Optional user assignment or override.
        /// </summary>
        public long? UserId { get; set; }
    }
}
