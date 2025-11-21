using KinoAppShared.DTOs.Showtimes;

namespace KinoAppCore.Services
{
    public interface IMovieShowtimeService
    {
        Task<IReadOnlyList<MovieShowtimeDto>> GetTodayAsync();
        Task<IReadOnlyList<MovieShowtimeDto>> GetByDateAsync(DateTime date);
    }
}
