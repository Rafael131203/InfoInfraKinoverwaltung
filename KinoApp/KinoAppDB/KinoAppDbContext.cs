using KinoAppCore.Entities;
using Microsoft.EntityFrameworkCore;
using NMF.Models;

//using NMF.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL; // <-- needed for UseXminAsConcurrencyToken()

namespace KinoAppDB;

public class KinoAppDbContext : DbContext
{
    public KinoAppDbContext(DbContextOptions<KinoAppDbContext> options) : base(options) { }
    public DbSet<Kunde> Kunden => Set<Kunde>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Kunde>()
            .Ignore(k => k.Parent);
        //// Unique seat per show
        //b.Entity<Ticket>()
        //    .HasIndex(x => new { x.ShowId, x.SeatId })
        //    .IsUnique();

        //// PostgreSQL optimistic concurrency on xmin
        //b.Entity<Ticket>()
        //    .Property(nameof(Ticket.xmin))
        //    .IsRowVersion()
        //    .HasColumnName("xmin")
        //    .ValueGeneratedOnAddOrUpdate();

        //// Relationships
        //b.Entity<Ticket>()
        //    .HasOne(t => t.Show)
        //    .WithMany(s => s.Tickets)
        //    .HasForeignKey(t => t.ShowId)
        //    .OnDelete(DeleteBehavior.Cascade);


    }
}
