using KinoAppShared.Enums;

namespace KinoAppDB.Entities
{
    public class VorstellungEntity
    {
        public long Id { get; set; }
        public DateTime Datum { get; set; }
        public required VorstellungStatus Status { get; set; }
        public string? FilmId { get; set; }
        public FilmEntity Film { get; set; }
        public long? KinosaalId { get; set; }
        public  KinosaalEntity Kinosaal { get; set; }
    }
}
