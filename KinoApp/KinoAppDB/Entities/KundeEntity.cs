namespace KinoAppDB.Entities;

public class KundeEntity
{
    public long Id { get; set; }
    public string Vorname { get; set; } = null!;
    public string Nachname { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Passwort { get; set; } = null!;

    // 1:1 optional – either a Kunde has a cart or not
    public long? WarenkorbId { get; set; }
    public WarenkorbEntity? Warenkorb { get; set; }

    // 1:n tickets
    public ICollection<TicketEntity> Tickets { get; set; } = new List<TicketEntity>();
}