namespace KinoAppCore.Config
{
    /// <summary>
    /// Configuration options for accessing the IMDb API.
    /// </summary>
    public class ApiOptions
    {
        /// <summary>
        /// The configuration section name used for binding IMDb API settings.
        /// </summary>
        public const string SectionName = "ImdbApi";

        /// <summary>
        /// The base URL of the IMDb API added with dependancy injection.
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;
    }
}
