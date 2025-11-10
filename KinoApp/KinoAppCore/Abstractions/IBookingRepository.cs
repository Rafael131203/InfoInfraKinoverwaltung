using KinoAppCore.Entities;
namespace KinoAppCore.Abstractions;
public interface IBookingRepository
{
    Task<Booking> GetAsync(Guid bookingId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}