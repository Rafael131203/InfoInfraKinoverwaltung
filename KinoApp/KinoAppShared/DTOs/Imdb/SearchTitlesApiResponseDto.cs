namespace KinoAppShared.DTOs.Imdb
{
    /// <summary>
    /// Response DTO for searching IMDb titles.
    /// </summary>
    public class SearchTitlesApiResponseDto
    {
        /// <summary>
        /// List of matching titles.
        /// </summary>
        public List<ImdbTitleApiDto> Titles { get; set; } = new();

        /// <summary>
        /// Total number of matching titles.
        /// </summary>
        public int? TotalCount { get; set; }

        /// <summary>
        /// Token for fetching the next page of results.
        /// </summary>
        public string? NextPageToken { get; set; }
    }
}
