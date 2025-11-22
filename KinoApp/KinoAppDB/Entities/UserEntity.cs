namespace KinoAppDB.Entities;

public class UserEntity
{
    public long Id { get; set; }
    public string Vorname { get; set; } = null!;
    public string Nachname { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Passwort { get; set; } = null!;
    public string Role { get; set; } = "User";

    public ICollection<TicketEntity> Tickets { get; set; } = new List<TicketEntity>();
}