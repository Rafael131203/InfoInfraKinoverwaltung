using Microsoft.EntityFrameworkCore;

namespace KinoAppDB;

public class KinoAppDbContext : DbContext
{
    public KinoAppDbContext(DbContextOptions<KinoAppDbContext> options) : base(options) { }

    // DbSets
    public DbSet<Entities.KundeEntity> Kunden => Set<Entities.KundeEntity>();
    public DbSet<Entities.TicketEntity> Tickets => Set<Entities.TicketEntity>();
    public DbSet<Entities.KinosaalEntity> Kinosaal => Set<Entities.KinosaalEntity>();
    public DbSet<Entities.SitzreiheEntity> Sitzreihe => Set<Entities.SitzreiheEntity>();
    public DbSet<Entities.SitzplatzEntity> Sitzplatz => Set<Entities.SitzplatzEntity>();
    public DbSet<Entities.FilmEntity> Film => Set<Entities.FilmEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Das hier ist der wichtigste Befehl:
        // Er sucht automatisch nach deinen Klassen "TicketEntityConfig" und "KundeEntityConfig"
        // und wendet die Regeln an, die wir dort gerade korrigiert haben.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KinoAppDbContext).Assembly);
    }
}