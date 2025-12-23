using KinoAppShared.DTOs.Vorstellung;

namespace KinoAppCore.Services
{
    /// <summary>
    /// Provides operations for managing showings (Vorstellungen).
    /// </summary>
    /// <remarks>
    /// A Vorstellung represents a scheduled screening of a film in a specific auditorium at a specific time.
    /// Implementations typically coordinate persistence and may enforce scheduling constraints.
    /// </remarks>
    public interface IVorstellungService
    {
        /// <summary>
        /// Creates a new showing.
        /// </summary>
        /// <param name="vorstellung">Create request.</param>
        /// <param name="ct">Cancellation token.</param>
        Task CreateVorstellungAsync(CreateVorstellungDTO vorstellung, CancellationToken ct);

        /// <summary>
        /// Updates an existing showing.
        /// </summary>
        /// <param name="vorstellung">Update request.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The update result, or <c>null</c> if the showing was not found.</returns>
        Task<UpdateVorstellungResultDTO?> UpdateVorstellungAsync(UpdateVorstellungDTO vorstellung, CancellationToken ct);

        /// <summary>
        /// Returns all showings scheduled for the specified day.
        /// </summary>
        /// <param name="datum">The calendar day to query.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<List<VorstellungDTO>> GetVorstellungVonTagAsync(DateTime datum, CancellationToken ct);

        /// <summary>
        /// Returns all showings for the specified auditorium.
        /// </summary>
        /// <param name="KinosaalId">Auditorium identifier.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<List<VorstellungDTO>> GetVorstellungVonKinosaalAsync(long KinosaalId, CancellationToken ct);

        /// <summary>
        /// Returns all showings for the specified auditorium scheduled on the specified day.
        /// </summary>
        /// <param name="datum">The calendar day to query.</param>
        /// <param name="KinosaalId">Auditorium identifier.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<List<VorstellungDTO>> GetVorstellungVonKinosaalUndTagAsync(DateTime datum, long KinosaalId, CancellationToken ct);

        /// <summary>
        /// Returns all showings for a given film.
        /// </summary>
        /// <param name="filmId">Film identifier.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<List<VorstellungDTO>> GetVorstellungVonFilm(string filmId, CancellationToken ct);

        /// <summary>
        /// Deletes a showing by its identifier.
        /// </summary>
        /// <param name="id">Showing identifier.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns><c>true</c> if the showing was deleted; otherwise <c>false</c>.</returns>
        Task<bool> DeleteVorstellungAsync(long id, CancellationToken ct);

        /// <summary>
        /// Returns all showings.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        Task<List<VorstellungDTO>> GetAlleVorstellungenAsync(CancellationToken ct);
    }
}
