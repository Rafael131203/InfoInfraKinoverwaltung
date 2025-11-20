// KinoAppDB/KinoAppDbContext.cs
using Microsoft.EntityFrameworkCore;

namespace KinoAppDB;

public class KinoAppDbContext : DbContext
{
    public KinoAppDbContext(DbContextOptions<KinoAppDbContext> options) : base(options) { }

    // DbSets only for EF entities
    public DbSet<Entities.KundeEntity> Kunden => Set<Entities.KundeEntity>();
    public DbSet<Entities.WarenkorbEntity> Warenkoerbe => Set<Entities.WarenkorbEntity>();
    public DbSet<Entities.TicketEntity> Tickets => Set<Entities.TicketEntity>();

    public DbSet<Entities.KinosaalEntity> Kinosaal => Set<Entities.KinosaalEntity>();
    public DbSet<Entities.SitzreiheEntity> Sitzreihe => Set<Entities.SitzreiheEntity>();
    public DbSet<Entities.SitzplatzEntity> Sitzplatz => Set<Entities.SitzplatzEntity>();
    public DbSet<Entities.FilmEntity> Film => Set<Entities.FilmEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Discover all IEntityTypeConfiguration<T> in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KinoAppDbContext).Assembly);
    }
}
