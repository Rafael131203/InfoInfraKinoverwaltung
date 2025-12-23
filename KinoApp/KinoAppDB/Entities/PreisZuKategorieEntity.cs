using KinoAppShared.Enums;

namespace KinoAppDB.Entities
{
    /// <summary>
    /// Database entity representing the configured price for a seat row category.
    /// </summary>
    /// <remarks>
    /// This table provides a single price per <see cref="SitzreihenKategorie"/> and is used to initialize and
    /// update seat prices for all seat rows assigned to that category.
    /// </remarks>
    public class PreisZuKategorieEntity
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Seat row category the price applies to.
        /// </summary>
        public SitzreihenKategorie Kategorie { get; set; }

        /// <summary>
        /// Price applied to seats within the category.
        /// </summary>
        public decimal Preis { get; set; }
    }
}
