namespace KinoAppShared.DTOs.Imdb
{
    /// <summary>
    /// Image metadata returned by the IMDb API.
    /// </summary>
    public class ImdbImageApiDto
    {
        /// <summary>
        /// URL of the image.
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Image width in pixels.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Image height in pixels.
        /// </summary>
        public int? Height { get; set; }
    }
}
