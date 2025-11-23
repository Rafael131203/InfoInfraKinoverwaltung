using KinoAppShared.Enums;

namespace KinoAppShared.DTOs.Showtimes
{
    public class SelectedSeatClientDto
    {
        public long SeatId { get; set; }
        public long VorstellungId { get; set; }
        public string RowLabel { get; set; } = string.Empty;
        public int SeatNumber { get; set; }
        public decimal Price { get; set; }

        public SitzreihenKategorie Category { get; set; }   // NEW
    }
}
