using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppShared.Messaging
{
    public record KundeRegistered
    (
    long KundeId,   // später Ändern in GUID?
    string Email,
    string Vorname,
    string Nachname,
    DateTime RegisteredAtUtc,
    string Role
    );

    public record ShowCreated
    (
      long ShowId,    // später Ändern in GUID?
      int FilmId,
      int KinosaalId,
      DateTime StartTimeUtc,
      int AvailableSeats
    );

    public record TicketSold
    (
        long TicketId,  // später Ändern in GUID?
        long ShowId,
        int Quantity,
        decimal TotalPrice,
        DateTime SoldAtUtc
    );

    public record TicketCancelled(
    long TicketId,
    long ShowId,
    decimal AmountToRefund, // Der Betrag, der abgezogen wird
    DateTime CancelledAtUtc
    );
}
