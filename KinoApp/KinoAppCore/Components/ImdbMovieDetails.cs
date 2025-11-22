namespace KinoAppCore.Components;

    public class ImdbMovieDetails
    {
        public string ImdbId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int? Year { get; set; }
        public string? Plot { get; set; }
        public string? PosterUrl { get; set; }
        public double? Rating { get; set; }
    }

