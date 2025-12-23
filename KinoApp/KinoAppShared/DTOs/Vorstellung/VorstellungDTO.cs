using KinoAppShared.DTOs.Imdb;
using KinoAppShared.DTOs.Kinosaal;

namespace KinoAppShared.DTOs.Vorstellung
{
    /// <summary>
    /// DTO representing a showtime including movie and auditorium details.
    /// </summary>
    public class VorstellungDTO
    {
        /// <summary>
        /// Showtime identifier.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Start date and time of the show.
        /// </summary>
        public DateTime Datum { get; set; }

        /// <summary>
        /// Movie being shown.
        /// </summary>
        public FilmDto Film { get; set; } = default!;

        /// <summary>
        /// Auditorium in which the show takes place.
        /// </summary>
        public KinosaalDTO Kinosaal { get; set; } = default!;
    }
}
