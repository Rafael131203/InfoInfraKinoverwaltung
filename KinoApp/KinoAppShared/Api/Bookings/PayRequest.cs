namespace KinoAppShared.Api.Bookings;
public record PayRequest(Guid BookingId, decimal Amount);