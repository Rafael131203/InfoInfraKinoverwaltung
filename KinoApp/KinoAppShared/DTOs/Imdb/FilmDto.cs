namespace KinoAppShared.DTOs.Imdb
{
    public class FilmDto
    {
        public string Id { get; set; }
        public string Titel { get; set; }
        public string? Beschreibung { get; set; }
        public int? Dauer { get; set; }
        public int? Fsk { get; set; }
        public string? Genre { get; set; }
        public string ImageURL { get; set; }
    }
}
