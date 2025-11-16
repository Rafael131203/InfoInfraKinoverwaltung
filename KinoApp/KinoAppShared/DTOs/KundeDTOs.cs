using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KinoAppShared.DTOs
{
    public class FullKundeDTO
    {
        public string Vorname { get; set; }
        public string Nachname { get; set; }
        public string Email { get; set; }
        public string Passwort { get; set; }   // Hinweis: In DTOs normalerweise NICHT senden
        public int Id { get; set; }

        public WarenkorbDTO Warenkorb { get; set; }
    }
    public class GetKundeDTO
    {
        public string Vorname { get; set; }
        public string Nachname { get; set; }
        public string Email { get; set; }
    }

}
