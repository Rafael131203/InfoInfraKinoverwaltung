namespace KinoAppDB.Entities;

public class KundeEntity
{
    public long Id { get; set; }
    public string Vorname { get; set; } = null!;
    public string Nachname { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Passwort { get; set; } = null!;

    public long? WarenkorbId { get; set; }

    public ICollection<TicketEntity> Tickets { get; set; } = new List<TicketEntity>();
}