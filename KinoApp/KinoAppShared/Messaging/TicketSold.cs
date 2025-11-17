namespace KinoAppShared.Messaging;

public class TicketSold
{
    public TicketSold() { } // für Deserialisierung

    public TicketSold(Guid showId, int quantity, decimal totalPrice)
    {
        ShowId = showId;
        Quantity = quantity;
        TotalPrice = totalPrice;
    }

    public Guid ShowId { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}