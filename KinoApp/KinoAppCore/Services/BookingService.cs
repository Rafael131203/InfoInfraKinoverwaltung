// Theatre.Core/Services/BookingService.cs
using KinoAppCore.Abstractions;
using KinoAppShared.Messaging;

namespace KinoAppCore.Services;

public sealed class BookingService : IBookingService
{
    private readonly IBookingRepository _repo;
    private readonly IMessageBus _bus;

    public BookingService(IBookingRepository repo, IMessageBus bus)
    { _repo = repo; _bus = bus; }

    public async Task PayAsync(Guid showId, Guid bookingId, decimal amount, CancellationToken ct)
    {
        var booking = await _repo.GetAsync(bookingId, ct);
        booking.Pay(amount);                      // business rule
        await _repo.SaveChangesAsync(ct);         // persist in SQL
        await _bus.PublishAsync(new TicketSold(showId, bookingId, 1, amount), ct); // event for stats
    }
}
