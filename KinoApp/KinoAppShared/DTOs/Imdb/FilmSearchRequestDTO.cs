namespace KinoAppShared.DTOs.Film
{
    public class FilmSearchRequestDTO
    {
        public string? Query { get; set; }
        public string? Genre { get; set; }
        public string SortBy { get; set; } = "Titel";
        public string SortOrder { get; set; } = "ASC";
    }
}
