namespace KinoAppShared.DTOs.Imdb
{
    /// <summary>
    /// Certificate / age rating information returned by the IMDb API.
    /// </summary>
    public class ImdbCertificateApiDto
    {
        /// <summary>
        /// Rating label (e.g. "FSK 16", "PG-13").
        /// </summary>
        public string? Rating { get; set; }

        /// <summary>
        /// Country for which the rating applies.
        /// </summary>
        public ImdbCountryApiDto? Country { get; set; }

        /// <summary>
        /// Additional attributes or flags provided by IMDb.
        /// </summary>
        public List<string>? Attributes { get; set; }
    }
}
