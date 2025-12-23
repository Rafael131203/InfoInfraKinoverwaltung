using KinoAppShared.Enums;

namespace KinoAppShared.DTOs.Kinosaal
{
    /// <summary>
    /// Request DTO for defining or updating a price per seating category.
    /// </summary>
    public class SetPreisDTO
    {
        /// <summary>
        /// Seating category.
        /// </summary>
        public SitzreihenKategorie Kategorie { get; set; }

        /// <summary>
        /// Price assigned to the category.
        /// </summary>
        public decimal Preis { get; set; }
    }
}
