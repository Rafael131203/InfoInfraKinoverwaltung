namespace KinoAppShared.DTOs.Kinosaal
{
    /// <summary>
    /// DTO representing an auditorium including its seating layout.
    /// </summary>
    public class KinosaalDTO
    {
        /// <summary>
        /// Auditorium identifier.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Auditorium name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Seat rows belonging to this auditorium.
        /// </summary>
        public List<SitzreiheDTO> Sitzreihen { get; set; } = new();
    }
}
