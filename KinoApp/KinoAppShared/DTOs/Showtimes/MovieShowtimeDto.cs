using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppShared.DTOs.Showtimes
{
    public class MovieShowtimeDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Tagline { get; set; } = string.Empty;  // optional short line
        public string Description { get; set; } = string.Empty;
        public string PosterUrl { get; set; } = string.Empty;

        public int DurationMinutes { get; set; }          // e.g. 128
        public string AgeRating { get; set; } = "FSK 12"; // e.g. "FSK 12"
        public string Genres { get; set; } = string.Empty; // e.g. "Sci-Fi · Thriller"

        public IReadOnlyList<ShowtimeDto> Showtimes { get; set; } = Array.Empty<ShowtimeDto>();
    }
}
