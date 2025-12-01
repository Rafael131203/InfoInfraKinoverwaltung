namespace KinoAppDB.Entities;

public class TicketEntity
{
    public long Id { get; set; }
    public int Status { get; set; }           // FK to LUT if you add it

    public long VorstellungId { get; set; }     // show
    // public VorstellungEntity Vorstellung { get; set; } = null!; // add later when you have it

    public long SitzplatzId { get; set; }       // seat
    // public SitzplatzEntity Sitzplatz { get; set; } = null!;     // add later when you have it

    public virtual SitzplatzEntity? Sitzplatz { get; set; }

    public long? UserId { get; set; }
    public UserEntity? User { get; set; }

}