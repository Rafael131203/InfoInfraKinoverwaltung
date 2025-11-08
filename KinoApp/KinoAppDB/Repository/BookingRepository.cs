using System;
using System.Threading;
using System.Threading.Tasks;
using KinoAppCore.Abstractions;
using KinoAppCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace KinoAppDB.Repository
{
    /// <summary>
    /// Handles persistence and retrieval of Booking entities using EF Core.
    /// </summary>
    public sealed class BookingRepository : IBookingRepository
    {
        private readonly KinoAppDbContext _db;

        public BookingRepository(KinoAppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Fetches a booking by its unique ID. Throws if not found.
        /// </summary>
        public async Task<Booking> GetAsync(Guid bookingId, CancellationToken ct)
        {
            var booking = await _db.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId, ct);

            if (booking is null)
                throw new InvalidOperationException($"Booking with ID {bookingId} not found.");

            return booking;
        }

        /// <summary>
        /// Saves all tracked entity changes to the database.
        /// </summary>
        public async Task SaveChangesAsync(CancellationToken ct)
        {
            await _db.SaveChangesAsync(ct);
        }
    }
}
