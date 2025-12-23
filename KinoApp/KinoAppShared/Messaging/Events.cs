

namespace KinoAppShared.Messaging
{
    /// <summary>
    /// Event published when a new user has been registered.
    /// </summary>
    public record KundeRegistered
    (long KundeId, string Email,string Vorname,string Nachname,DateTime RegisteredAtUtc,string Role);
    /// <summary>
    /// Event published when a new showtime is created.
    /// </summary>
    public record ShowCreated(long ShowId,int FilmId,int KinosaalId,DateTime StartTimeUtc,int AvailableSeats);
    /// <summary>
    /// Event published when one or more tickets are sold.
    /// </summary>
    public record TicketSold(long TicketId, long ShowId,int Quantity,decimal TotalPrice,DateTime SoldAtUtc);
    /// <summary>
    /// Event published when a ticket is cancelled.
    /// </summary>
    public record TicketCancelled(long TicketId,long ShowId,decimal AmountToRefund, DateTime CancelledAtUtc);
}
