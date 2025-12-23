using KinoAppDB.Entities;
using KinoAppShared.DTOs.Kinosaal;

namespace KinoAppCore.Services
{
    /// <summary>
    /// Provides auditorium (Kinosaal) operations including creation, retrieval, and seating category updates.
    /// </summary>
    public interface IKinosaalService
    {
        /// <summary>
        /// Creates a new auditorium including its seat rows and seats.
        /// </summary>
        /// <param name="kinosaal">Basic auditorium data.</param>
        /// <param name="AnzahlSitzreihen">Number of seat rows to create.</param>
        /// <param name="GrößeSitzreihen">Number of seats per row.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The ID of the created auditorium.</returns>
        Task<long> CreateAsync(CreateKinosaalDTO kinosaal, int AnzahlSitzreihen, int GrößeSitzreihen, CancellationToken ct = default);

        /// <summary>
        /// Retrieves an auditorium and its seating layout, optionally enriched with ticket status for a showing.
        /// </summary>
        /// <param name="id">Auditorium ID.</param>
        /// <param name="vorstellungId">
        /// Optional showing ID. When provided, seat status is derived from tickets for that showing.
        /// </param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The auditorium DTO if found; otherwise <c>null</c>.</returns>
        Task<KinosaalDTO?> GetKinosaalAsync(long id, long? vorstellungId, CancellationToken ct);

        /// <summary>
        /// Changes the category of a seat row and updates seat prices accordingly.
        /// </summary>
        /// <param name="dto">Category change request.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The updated seat row entity if found; otherwise <c>null</c>.</returns>
        Task<SitzreiheEntity?> ChangeSitzreiheKategorieAsync(ChangeKategorieSitzreiheDTO dto, CancellationToken ct);

        /// <summary>
        /// Deletes an auditorium by its ID.
        /// </summary>
        /// <param name="id">Auditorium ID.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The deleted entity if it existed; otherwise <c>null</c>.</returns>
        Task<KinosaalEntity?> DeleteAsync(long id, CancellationToken ct = default);
    }
}
