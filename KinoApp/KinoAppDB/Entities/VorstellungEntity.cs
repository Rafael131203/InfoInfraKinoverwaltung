using KinoAppShared.Enums;

namespace KinoAppDB.Entities
{
    /// <summary>
    /// Database entity representing a scheduled showing (Vorstellung).
    /// </summary>
    /// <remarks>
    /// A showing schedules a film in an auditorium at a specific time and has a lifecycle represented by
    /// <see cref="VorstellungStatus"/>.
    /// </remarks>
    public class VorstellungEntity
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Start date and time of the showing.
        /// </summary>
        public DateTime Datum { get; set; }

        /// <summary>
        /// Current status of the showing.
        /// </summary>
        public required VorstellungStatus Status { get; set; }

        /// <summary>
        /// Foreign key to the film being shown.
        /// </summary>
        public string? FilmId { get; set; }

        /// <summary>
        /// Navigation property to the film.
        /// </summary>
        public FilmEntity Film { get; set; } = null!;

        /// <summary>
        /// Foreign key to the auditorium where the film is shown.
        /// </summary>
        public long? KinosaalId { get; set; }

        /// <summary>
        /// Navigation property to the auditorium.
        /// </summary>
        public KinosaalEntity Kinosaal { get; set; } = null!;
    }
}
