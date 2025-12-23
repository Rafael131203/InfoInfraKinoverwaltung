namespace KinoAppShared.DTOs.Imdb
{
    /// <summary>
    /// Country information as returned by the IMDb API.
    /// </summary>
    public class ImdbCountryApiDto
    {
        /// <summary>
        /// ISO country code (e.g. "DE").
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// Human-readable country name (e.g. "Germany").
        /// </summary>
        public string? Name { get; set; }
    }
}
