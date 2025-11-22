using System.ComponentModel.DataAnnotations;

namespace KinoAppShared.DTOs
{
    public class BuyTicketDTO
    {
        public long VorstellungId { get; set; }
        public long SitzplatzId { get; set; }

        [Range(1, 10, ErrorMessage = "Man muss mindestens 1 Ticket kaufen!")]
        public int Anzahl { get; set; } = 1;

        public decimal PreisVorschlag { get; set; }

        // WICHTIG: Das Feld für den Gast-Kauf.
        // Es ist nullable (string?), weil eingeloggte User es leer lassen.
        public string? GastEmail { get; set; }
    }
}