using KinoAppShared.DTOs.Vorstellung;

namespace KinoAppWeb.Services
{
    /// <summary>
    /// Client-side API wrapper for showtime management endpoints.
    /// </summary>
    public interface IVorstellungService
    {
        /// <summary>
        /// Creates a new showtime.
        /// </summary>
        Task CreateVorstellungAsync(CreateVorstellungDTO vorstellung, CancellationToken ct);

        /// <summary>
        /// Updates an existing showtime and returns both the previous and updated state.
        /// </summary>
        Task<UpdateVorstellungResultDTO?> UpdateVorstellungAsync(UpdateVorstellungDTO vorstellung, CancellationToken ct);

        /// <summary>
        /// Returns all showtimes for the specified day.
        /// </summary>
        Task<List<VorstellungDTO>> GetVorstellungVonTagAsync(DateTime datum, CancellationToken ct);

        /// <summary>
        /// Returns all showtimes for the specified auditorium.
        /// </summary>
        Task<List<VorstellungDTO>> GetVorstellungVonKinosaalAsync(long KinosaalId, CancellationToken ct);

        /// <summary>
        /// Returns all showtimes for the specified auditorium on the specified day.
        /// </summary>
        Task<List<VorstellungDTO>> GetVorstellungVonKinosaalUndTagAsync(DateTime datum, long KinosaalId, CancellationToken ct);

        /// <summary>
        /// Returns all showtimes for the specified film.
        /// </summary>
        Task<List<VorstellungDTO>> GetVorstellungVonFilm(string filmId, CancellationToken ct);

        /// <summary>
        /// Deletes a showtime by its identifier.
        /// </summary>
        Task<bool> DeleteVorstellungAsync(long id, CancellationToken ct);

        /// <summary>
        /// Returns all showtimes.
        /// </summary>
        Task<List<VorstellungDTO>> GetAlleVorstellungenAsync();
    }
}
