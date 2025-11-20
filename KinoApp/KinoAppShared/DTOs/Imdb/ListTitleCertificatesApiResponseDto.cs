// ListTitleCertificatesApiResponseDto.cs
namespace KinoAppShared.DTOs.Imdb
{
    public class ListTitleCertificatesApiResponseDto
    {
        public List<ImdbCertificateApiDto>? Certificates { get; set; }
        public int? TotalCount { get; set; }
    }
}
