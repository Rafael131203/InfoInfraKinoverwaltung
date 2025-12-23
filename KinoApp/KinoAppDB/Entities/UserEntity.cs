namespace KinoAppDB.Entities
{
    /// <summary>
    /// Database entity representing an application user.
    /// </summary>
    /// <remarks>
    /// Stores authentication credentials and basic profile data. Tickets are associated to a user when
    /// reserved or booked; for guest purchases/reservations the <see cref="Tickets"/> relationship may remain empty.
    /// </remarks>
    public class UserEntity
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// User's first name.
        /// </summary>
        public string Vorname { get; set; } = null!;

        /// <summary>
        /// User's last name.
        /// </summary>
        public string Nachname { get; set; } = null!;

        /// <summary>
        /// Email address used as the unique login identifier.
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Password hash (never store plain text passwords).
        /// </summary>
        public string Passwort { get; set; } = null!;

        /// <summary>
        /// Role name assigned to the user.
        /// </summary>
        public string Role { get; set; } = "User";

        /// <summary>
        /// Tickets reserved or booked by the user.
        /// </summary>
        public ICollection<TicketEntity> Tickets { get; set; } = new List<TicketEntity>();
    }
}
