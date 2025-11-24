using System.ComponentModel.DataAnnotations;
using System.Collections.Generic; // Für List<>

namespace KinoAppShared.DTOs
{
    public class BuyTicketDTO
    {
        public long VorstellungId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Mindestens ein Sitzplatz muss gewählt werden.")]
        public List<long> SitzplatzIds { get; set; } = new();

        public string? GastEmail { get; set; }

        // --- HIER DEN TRICK EINFÜGEN ---
        // Das berechnet die Anzahl automatisch, damit der Controller nicht meckert.
        // (Wird nicht in der Datenbank gespeichert, ist nur ein Helfer)
        public int Anzahl => SitzplatzIds.Count;
    }
}