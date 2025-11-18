namespace KinoAppCore.Components
{
    public class ImdbMovieSearchResult
    {
        public string ImdbId { get; set; } = string.Empty;

        public string? Type { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? OriginalTitle { get; set; }

        public int? Year { get; set; }

        public int? RuntimeSeconds { get; set; }

        public List<string> Genres { get; set; } = new();

        public double? Rating { get; set; }
        public int? VoteCount { get; set; }

        public string? Plot { get; set; }

        public string? PosterUrl { get; set; }
        public int? PosterWidth { get; set; }
        public int? PosterHeight { get; set; }
    }
}
