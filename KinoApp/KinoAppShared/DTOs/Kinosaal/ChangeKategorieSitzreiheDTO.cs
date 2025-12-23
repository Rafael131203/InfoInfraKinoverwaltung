using KinoAppShared.Enums;

namespace KinoAppShared.DTOs.Kinosaal
{
    /// <summary>
    /// Request DTO for changing the category of a seat row.
    /// </summary>
    public class ChangeKategorieSitzreiheDTO
    {
        /// <summary>
        /// Identifier of the seat row.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// New seating category.
        /// </summary>
        public SitzreihenKategorie Kategorie { get; set; }
    }
}
