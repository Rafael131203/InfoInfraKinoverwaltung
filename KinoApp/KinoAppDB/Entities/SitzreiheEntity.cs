using KinoAppShared.Enums;

namespace KinoAppDB.Entities
{
    /// <summary>
    /// Database entity representing a seat row within an auditorium.
    /// </summary>
    /// <remarks>
    /// A seat row belongs to an optional <see cref="KinosaalEntity"/> and contains multiple seats
    /// (<see cref="SitzplatzEntity"/>). The <see cref="Kategorie"/> influences pricing and UI presentation.
    /// </remarks>
    public class SitzreiheEntity
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Category assigned to the seat row (e.g., Parkett).
        /// </summary>
        public SitzreihenKategorie Kategorie { get; set; }

        /// <summary>
        /// Display label for the row (e.g., "Reihe 1").
        /// </summary>
        public string Bezeichnung { get; set; } = string.Empty;

        /// <summary>
        /// Foreign key to the owning auditorium.
        /// </summary>
        public long? KinosaalId { get; set; }

        /// <summary>
        /// Navigation property to the owning auditorium.
        /// </summary>
        public KinosaalEntity? Kinosaal { get; set; }

        /// <summary>
        /// Seats contained in this row.
        /// </summary>
        public ICollection<SitzplatzEntity> Sitzplätze { get; set; } = new List<SitzplatzEntity>();
    }
}
