// ImdbCertificateApiDto.cs
namespace KinoAppShared.DTOs.Imdb
{
    public class ImdbCertificateApiDto
    {
        public string? Rating { get; set; }              // e.g. "FSK 16", "PG-13"
        public ImdbCountryApiDto? Country { get; set; }  // DE / US / ...
        public List<string>? Attributes { get; set; }    // extra flags, can be ignored
    }
}
