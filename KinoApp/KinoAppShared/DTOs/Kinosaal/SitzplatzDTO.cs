using KinoAppShared.Enums;

namespace KinoAppShared.DTOs.Kinosaal
{
    public class SitzplatzDTO
    {
        public long Id { get; set; }
        public int Nummer { get; set; }
        public decimal Preis { get; set; }
        public Boolean Gebucht { get; set; }
        public TicketStatus Status { get; set; }
    }
}
