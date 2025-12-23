namespace KinoAppShared.DTOs.Imdb
{
    /// <summary>
    /// Lightweight film DTO used for API responses and listings.
    /// </summary>
    public class FilmDto
    {
        /// <summary>
        /// Film identifier (IMDb ID or internal ID).
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Display title of the film.
        /// </summary>
        public string Titel { get; set; } = string.Empty;

        /// <summary>
        /// Short plot or description.
        /// </summary>
        public string? Beschreibung { get; set; }

        /// <summary>
        /// Runtime in minutes (or seconds, depending on source).
        /// </summary>
        public int? Dauer { get; set; }

        /// <summary>
        /// Age rating (FSK) if available.
        /// </summary>
        public int? Fsk { get; set; }

        /// <summary>
        /// Primary genre of the film.
        /// </summary>
        public string? Genre { get; set; }

        /// <summary>
        /// URL of the primary poster or image.
        /// </summary>
        public string ImageURL { get; set; } = string.Empty;
    }
}
