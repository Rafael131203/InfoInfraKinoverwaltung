namespace KinoAppShared.DTOs.Ticket
{
    public class UpdateTicketStatusDTO
    {
        public long TicketId { get; set; }
        public string Status { get; set; } = default!;
        public long? UserId { get; set; }  // optional, for admin override / reassign
    }

}
