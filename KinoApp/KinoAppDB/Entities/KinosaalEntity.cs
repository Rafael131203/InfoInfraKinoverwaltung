namespace KinoAppDB.Entities
{
    /// <summary>
    /// Database entity representing an auditorium (Kinosaal).
    /// </summary>
    /// <remarks>
    /// A <see cref="KinosaalEntity"/> contains multiple seat rows (<see cref="SitzreiheEntity"/>)
    /// and can host multiple showings (<see cref="VorstellungEntity"/>).
    /// </remarks>
    public class KinosaalEntity
    {
        /// <summary>
        /// Primary key of the auditorium.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Display name of the auditorium.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Seat rows belonging to this auditorium.
        /// </summary>
        public ICollection<SitzreiheEntity> Sitzreihen { get; set; } = new List<SitzreiheEntity>();

        /// <summary>
        /// Showings scheduled in this auditorium.
        /// </summary>
        public ICollection<VorstellungEntity> Vorstellungen { get; set; } = new List<VorstellungEntity>();
    }
}
