namespace KinoAppShared.DTOs
{
    public class FullKundeDTO
    {
        public required string Vorname { get; set; }
        public required string Nachname { get; set; }
        public required string Email { get; set; }
        public required string Passwort { get; set; }   // Hinweis: In DTOs normalerweise NICHT senden
        public int Id { get; set; }

        public WarenkorbDTO? Warenkorb { get; set; }
    }
    public class GetKundeDTO
    {
        public string? Vorname { get; set; }
        public string? Nachname { get; set; }
        public required string Email { get; set; }
    }

}
