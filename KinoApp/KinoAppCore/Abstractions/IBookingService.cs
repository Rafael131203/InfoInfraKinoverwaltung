namespace KinoAppCore.Abstractions;
public interface IBookingService
{
    Task PayAsync(Guid showId, Guid bookingId, decimal amount, CancellationToken ct);
}
