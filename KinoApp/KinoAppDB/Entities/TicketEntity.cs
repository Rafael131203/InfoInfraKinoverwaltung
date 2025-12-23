namespace KinoAppDB.Entities
{
    /// <summary>
    /// Database entity representing a ticket for a specific seat in a specific showing.
    /// </summary>
    /// <remarks>
    /// A ticket is uniquely identified by the combination of <see cref="VorstellungId"/> and <see cref="SitzplatzId"/>
    /// (enforced via a unique index). The <see cref="Status"/> is stored as an integer to align with shared enum values.
    /// </remarks>
    public class TicketEntity
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Ticket status stored as an integer (mapped from the shared ticket status enum).
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Foreign key to the showing (Vorstellung).
        /// </summary>
        public long VorstellungId { get; set; }

        /// <summary>
        /// Foreign key to the seat (Sitzplatz).
        /// </summary>
        public long SitzplatzId { get; set; }

        /// <summary>
        /// Navigation property to the seat.
        /// </summary>
        public virtual SitzplatzEntity? Sitzplatz { get; set; }

        /// <summary>
        /// Optional foreign key to the user who reserved or booked the ticket.
        /// </summary>
        public long? UserId { get; set; }

        /// <summary>
        /// Navigation property to the associated user.
        /// </summary>
        public UserEntity? User { get; set; }
    }
}
