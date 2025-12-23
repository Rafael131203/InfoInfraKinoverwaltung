namespace KinoAppShared.DTOs.Vorstellung
{
    /// <summary>
    /// Request DTO for updating an existing showtime.
    /// </summary>
    public class UpdateVorstellungDTO
    {
        /// <summary>
        /// Showtime identifier.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Optional new movie identifier.
        /// </summary>
        public string? FilmId { get; set; }

        /// <summary>
        /// Optional new start date and time.
        /// </summary>
        public DateTime? Datum { get; set; }
    }
}
