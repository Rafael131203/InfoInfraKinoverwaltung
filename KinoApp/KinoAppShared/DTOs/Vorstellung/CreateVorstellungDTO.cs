using KinoAppShared.Enums;

namespace KinoAppShared.DTOs.Vorstellung
{
    /// <summary>
    /// Request DTO for creating a new showtime.
    /// </summary>
    public class CreateVorstellungDTO
    {
        /// <summary>
        /// Start date and time of the show.
        /// </summary>
        public DateTime Datum { get; set; }

        /// <summary>
        /// Initial status of the showtime.
        /// </summary>
        public VorstellungStatus Status { get; set; }

        /// <summary>
        /// Identifier of the movie being shown.
        /// </summary>
        public string FilmId { get; set; } = string.Empty;

        /// <summary>
        /// Auditorium in which the show takes place.
        /// </summary>
        public long KinosaalId { get; set; }
    }
}
