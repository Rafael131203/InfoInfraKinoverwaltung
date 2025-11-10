using Microsoft.EntityFrameworkCore;
using KinoAppCore.Entities;
using Npgsql.EntityFrameworkCore.PostgreSQL; // <-- needed for UseXminAsConcurrencyToken()

namespace KinoAppDB;

public class KinoAppDbContext : DbContext
{
    public KinoAppDbContext(DbContextOptions<KinoAppDbContext> options) : base(options) { }

    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Show> Shows => Set<Show>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Unique seat per show
        b.Entity<Ticket>()
            .HasIndex(x => new { x.ShowId, x.SeatId })
            .IsUnique();

        // PostgreSQL optimistic concurrency on xmin
        b.Entity<Ticket>()
            .Property(nameof(Ticket.xmin))
            .IsRowVersion()
            .HasColumnName("xmin")
            .ValueGeneratedOnAddOrUpdate();

        // Relationships
        b.Entity<Ticket>()
            .HasOne(t => t.Show)
            .WithMany(s => s.Tickets)
            .HasForeignKey(t => t.ShowId)
            .OnDelete(DeleteBehavior.Cascade);

        // Constraints
        b.Entity<Show>()
            .Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(200);

        b.Entity<User>(e =>
        {
            e.HasIndex(x => x.Username).IsUnique();
            e.Property(x => x.Username).HasMaxLength(100).IsRequired();
            e.Property(x => x.PasswordHash).IsRequired();
        });

        b.Entity<Booking>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.AmountPaid).HasPrecision(10, 2);
        });
    }
}
