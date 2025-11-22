using KinoAppShared.DTOs.Imdb;
using KinoAppShared.DTOs.Kinosaal;

namespace KinoAppShared.DTOs.Vorstellung
{
    public class VorstellungDTO
    {
        public long Id { get; set; }
        public DateTime Datum { get; set; }
        public FilmDto Film { get; set; }
        public KinosaalDTO Kinosaal { get; set; }
    }
}
