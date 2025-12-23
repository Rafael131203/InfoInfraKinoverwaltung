using KinoAppDB.Entities;
using KinoAppShared.DTOs.Kinosaal;

namespace KinoAppWeb.Services
{
    /// <summary>
    /// Client-side API wrapper for auditorium management endpoints.
    /// </summary>
    public interface IKinosaalService
    {
        /// <summary>
        /// Creates a new auditorium including generated seat rows and seats.
        /// </summary>
        /// <param name="kinosaal">Auditorium payload (name).</param>
        /// <param name="AnzahlSitzreihen">Number of rows to generate.</param>
        /// <param name="GrößeSitzreihen">Number of seats per row.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The identifier of the created auditorium.</returns>
        Task<long> CreateAsync(CreateKinosaalDTO kinosaal, int AnzahlSitzreihen, int GrößeSitzreihen, CancellationToken ct = default);

        /// <summary>
        /// Loads an auditorium including its seating layout.
        /// </summary>
        /// <param name="id">Auditorium identifier.</param>
        /// <param name="vorstellungId">
        /// Optional showtime identifier. When provided, seat status is returned (free/reserved/booked).
        /// </param>
        /// <param name="ct">Cancellation token.</param>
        Task<KinosaalDTO?> GetKinosaalAsync(long id, long? vorstellungId, CancellationToken ct);

        /// <summary>
        /// Changes the category of a seat row.
        /// </summary>
        /// <param name="dto">Category change request.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<SitzreiheEntity> ChangeSitzreiheKategorieAsync(ChangeKategorieSitzreiheDTO dto, CancellationToken ct);

        /// <summary>
        /// Deletes an auditorium by id.
        /// </summary>
        /// <param name="id">Auditorium identifier.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<KinosaalEntity?> DeleteAsync(long id, CancellationToken ct = default);
    }
}
