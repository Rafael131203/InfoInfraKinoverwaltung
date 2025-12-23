using System.ComponentModel.DataAnnotations;

namespace KinoAppShared.DTOs
{
    /// <summary>
    /// Request DTO for buying one or more tickets.
    /// </summary>
    public class BuyTicketDTO
    {
        /// <summary>
        /// Showtime identifier.
        /// </summary>
        public long VorstellungId { get; set; }

        /// <summary>
        /// Identifiers of the selected seats.
        /// </summary>
        [Required]
        [MinLength(1, ErrorMessage = "Mindestens ein Sitzplatz muss gewählt werden.")]
        public List<long> SitzplatzIds { get; set; } = new();

        /// <summary>
        /// Optional email address for guest purchases.
        /// </summary>
        public string? GastEmail { get; set; }

        /// <summary>
        /// Number of selected seats.
        /// </summary>
        public int Anzahl => SitzplatzIds.Count;
    }
}
