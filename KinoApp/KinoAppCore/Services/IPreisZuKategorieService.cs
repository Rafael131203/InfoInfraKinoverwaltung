using KinoAppDB.Entities;
using KinoAppShared.DTOs.Kinosaal;
using KinoAppShared.Enums;

namespace KinoAppCore.Services
{
    /// <summary>
    /// Manages price configuration for seat row categories.
    /// </summary>
    public interface IPreisZuKategorieService
    {
        /// <summary>
        /// Retrieves the configured price for the specified seat row category.
        /// </summary>
        /// <param name="kategorie">Seat row category.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The category price entity if configured; otherwise <c>null</c>.</returns>
        Task<PreisZuKategorieEntity?> GetPreisAsync(SitzreihenKategorie kategorie, CancellationToken ct);

        /// <summary>
        /// Sets the price for a seat row category and updates affected seats accordingly.
        /// </summary>
        /// <param name="setPreisDTO">Price update request.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The persisted category price entity.</returns>
        Task<PreisZuKategorieEntity> SetPreisAsync(SetPreisDTO setPreisDTO, CancellationToken ct);
    }
}
