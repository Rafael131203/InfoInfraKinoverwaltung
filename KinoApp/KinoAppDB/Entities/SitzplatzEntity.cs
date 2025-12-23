using System.Text.Json.Serialization;

namespace KinoAppDB.Entities
{
    /// <summary>
    /// Database entity representing a single seat within a seat row.
    /// </summary>
    /// <remarks>
    /// A seat belongs to exactly one <see cref="SitzreiheEntity"/> and stores its current price.
    /// </remarks>
    public class SitzplatzEntity
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Seat number within the auditorium layout.
        /// </summary>
        public int Nummer { get; set; }

        /// <summary>
        /// Current price for this seat.
        /// </summary>
        public decimal Preis { get; set; }

        /// <summary>
        /// Foreign key to the owning seat row.
        /// </summary>
        public long SitzreiheId { get; set; }

        /// <summary>
        /// Navigation property to the owning seat row.
        /// </summary>
        [JsonIgnore]
        public SitzreiheEntity? Sitzreihe { get; set; }
    }
}
