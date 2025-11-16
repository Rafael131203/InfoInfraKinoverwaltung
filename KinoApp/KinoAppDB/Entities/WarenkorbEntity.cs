namespace KinoAppDB.Entities;

public class WarenkorbEntity
{
    public long Id { get; set; }
    public decimal Gesamtpreis { get; set; }
    public short? Zahlungsmittel { get; set; }   // FK to LUT if you add it

    // owner – for your school project you probably only need Kunde (no Guest)
    public long KundeId { get; set; }
    public KundeEntity Kunde { get; set; } = null!;

    public ICollection<TicketEntity> Tickets { get; set; } = new List<TicketEntity>();
}