namespace KinoAppShared.DTOs.Ticket
{
    public class ReserveTicketDTO
    {
        public long VorstellungId { get; set; }
        public List<long> SitzplatzIds { get; set; } = new();
    }
}
