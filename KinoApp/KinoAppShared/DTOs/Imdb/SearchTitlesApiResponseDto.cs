using System;
namespace KinoAppShared.DTOs.Imdb
{
    public class SearchTitlesApiResponseDto
    {
        public List<ImdbTitleApiDto> Titles { get; set; } = new();
        public int? TotalCount { get; set; }
        public string? NextPageToken { get; set; }
    }
}
