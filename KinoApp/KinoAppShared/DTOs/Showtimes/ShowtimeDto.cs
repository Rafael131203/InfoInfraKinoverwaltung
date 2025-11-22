using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppShared.DTOs.Showtimes
{
    public class ShowtimeDto
    {
        public int Id { get; set; }
        public DateTime StartsAt { get; set; }
        public string ScreenName { get; set; } = string.Empty; // e.g. "Saal 3"
        public string Version { get; set; } = string.Empty;    // e.g. "OV", "3D"
    }
}
