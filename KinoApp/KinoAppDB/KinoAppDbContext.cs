// KinoAppDB/KinoAppDbContext.cs
using KinoAppDB.Entities;
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
        modelBuilder.Entity<VorstellungEntity>(entity =>
        {
            // Map enum <-> int for Status
            entity.Property(v => v.Status)
                  .HasConversion<int>();   // uses underlying int value of the enum
        });
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KinoAppDbContext).Assembly);
    }
}
