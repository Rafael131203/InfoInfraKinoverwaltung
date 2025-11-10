namespace KinoAppShared.Messaging;
public record TicketSold(Guid ShowId, Guid BookingId, int Quantity, decimal TotalPrice);
