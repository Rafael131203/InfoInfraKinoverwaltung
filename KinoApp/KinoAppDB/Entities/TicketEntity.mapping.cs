using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KinoAppDB.Entities;

public class TicketEntityConfig : IEntityTypeConfiguration<TicketEntity>
{
    public void Configure(EntityTypeBuilder<TicketEntity> b)
    {
        b.ToTable("ticket");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        b.Property(x => x.Status).HasColumnName("status").IsRequired();
        b.Property(x => x.VorstellungId).HasColumnName("vorstellung_id").IsRequired();
        b.Property(x => x.SitzplatzId).HasColumnName("sitzplatz_id").IsRequired();

        b.Property(x => x.KundeId).HasColumnName("kunde_id");
        b.Property(x => x.WarenkorbId).HasColumnName("warenkorb_id");

        // You’ll add real FKs when you add VorstellungEntity/SitzplatzEntity
        // b.HasOne(x => x.Vorstellung)...
        // b.HasOne(x => x.Sitzplatz)...

        b.HasOne(x => x.Kunde)
         .WithMany(k => k.Tickets)
         .HasForeignKey(x => x.KundeId)
         .OnDelete(DeleteBehavior.SetNull);

        b.HasOne(x => x.Warenkorb)
         .WithMany(w => w.Tickets)
         .HasForeignKey(x => x.WarenkorbId)
         .OnDelete(DeleteBehavior.SetNull);

        // unique seat per show
        b.HasIndex(x => new { x.VorstellungId, x.SitzplatzId }).IsUnique();
    }
}