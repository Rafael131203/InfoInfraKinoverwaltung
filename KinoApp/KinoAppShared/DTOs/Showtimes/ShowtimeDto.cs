namespace KinoAppShared.DTOs.Showtimes
{
    public class ShowtimeDto
    {
        /// <summary>
        /// The Vorstellung Id (primary key in the backend).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// When the show starts. Comes from VorstellungDTO.Datum.
        /// </summary>
        public DateTime StartsAt { get; set; }

        /// <summary>
        /// Kinosaal Id for this showtime.
        /// </summary>
        public long KinosaalId { get; set; }

        /// <summary>
        /// Display name of the Kinosaal (e.g. "Saal 3").
        /// </summary>
        public string KinosaalName { get; set; } = string.Empty;
    }
}
