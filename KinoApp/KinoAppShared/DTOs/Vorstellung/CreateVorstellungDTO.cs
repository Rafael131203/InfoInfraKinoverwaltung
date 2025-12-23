using KinoAppShared.Enums;

namespace KinoAppShared.DTOs.Vorstellung
{
    public class CreateVorstellungDTO
    {
        public DateTime Datum { get; set; }
        public VorstellungStatus Status { get; set; }
        public string FilmId { get; set; }
        public long KinosaalId {  get; set; }
    }
}
