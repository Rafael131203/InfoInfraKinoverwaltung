namespace KinoAppShared.DTOs.Imdb
{
    public class ImdbTitleApiDto
    {
        public string Id { get; set; } = string.Empty;
        public string? Type { get; set; }

        public string? PrimaryTitle { get; set; }
        public string? OriginalTitle { get; set; }

        public ImdbImageApiDto? PrimaryImage { get; set; }

        public int? StartYear { get; set; }
        public int? RuntimeSeconds { get; set; }

        public List<string>? Genres { get; set; }

        public ImdbRatingApiDto? Rating { get; set; }

        public string? Plot { get; set; }

        // (you can keep the other fields you already had: directors, writers, etc.)
    }
}
