using KinoAppShared.DTOs.Showtimes;

namespace KinoAppCore.Services
{
    /// <summary>
    /// Provides movie showtime views for a given day.
    /// </summary>
    /// <remarks>
    /// Implementations may query the backend API, an in-memory cache, or a database, but should return a
    /// read-optimized representation suitable for UI presentation.
    /// </remarks>
    public interface IMovieShowtimeService
    {
        /// <summary>
        /// Returns showtimes for the current day.
        /// </summary>
        Task<IReadOnlyList<MovieShowtimeDto>> GetTodayAsync();

        /// <summary>
        /// Returns showtimes for the specified day.
        /// </summary>
        /// <param name="date">The calendar day to query.</param>
        Task<IReadOnlyList<MovieShowtimeDto>> GetByDateAsync(DateTime date);
    }
}
