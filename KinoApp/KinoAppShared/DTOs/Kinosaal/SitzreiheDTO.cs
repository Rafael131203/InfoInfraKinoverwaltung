using KinoAppShared.Enums;

namespace KinoAppShared.DTOs.Kinosaal
{
    /// <summary>
    /// DTO representing a seat row.
    /// </summary>
    public class SitzreiheDTO
    {
        /// <summary>
        /// Seat row identifier.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Human-readable row label.
        /// </summary>
        public string Bezeichnung { get; set; } = string.Empty;

        /// <summary>
        /// Seating category of the row.
        /// </summary>
        public SitzreihenKategorie Kategorie { get; set; }

        /// <summary>
        /// Seats belonging to this row.
        /// </summary>
        public List<SitzplatzDTO> Sitzplätze { get; set; } = new();
    }
}
