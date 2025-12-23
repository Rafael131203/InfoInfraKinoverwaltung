namespace KinoAppShared.DTOs.Kinosaal
{
    /// <summary>
    /// Request DTO for creating a new auditorium.
    /// </summary>
    public class CreateKinosaalDTO
    {
        /// <summary>
        /// Display name of the auditorium.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
