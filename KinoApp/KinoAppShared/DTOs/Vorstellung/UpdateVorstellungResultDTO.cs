namespace KinoAppShared.DTOs.Vorstellung
{
    /// <summary>
    /// Result DTO describing a showtime update operation.
    /// </summary>
    public class UpdateVorstellungResultDTO
    {
        /// <summary>
        /// State of the showtime before the update.
        /// </summary>
        public VorstellungDTO VorstellungAlt { get; set; } = default!;

        /// <summary>
        /// State of the showtime after the update.
        /// </summary>
        public VorstellungDTO VorstellungNeu { get; set; } = default!;
    }
}
