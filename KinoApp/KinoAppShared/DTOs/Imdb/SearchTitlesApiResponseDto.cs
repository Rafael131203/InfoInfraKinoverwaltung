using System;
namespace KinoAppShared.DTOs.Imdb
{
    public class SearchTitlesApiResponseDto
    {
        public List<ImdbTitleApiDto> Titles { get; set; } = new();
    }
}
