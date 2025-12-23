namespace KinoAppShared.DTOs.Imdb
{
    /// <summary>
    /// Response DTO for the IMDb certificates endpoint.
    /// </summary>
    public class ListTitleCertificatesApiResponseDto
    {
        /// <summary>
        /// List of certificates associated with a title.
        /// </summary>
        public List<ImdbCertificateApiDto>? Certificates { get; set; }

        /// <summary>
        /// Total number of certificates returned by the API.
        /// </summary>
        public int? TotalCount { get; set; }
    }
}
