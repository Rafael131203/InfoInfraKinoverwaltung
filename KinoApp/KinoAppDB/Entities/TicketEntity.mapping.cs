using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KinoAppDB.Entities;

public class TicketEntityConfig : IEntityTypeConfiguration<TicketEntity>
{
    public void Configure(EntityTypeBuilder<TicketEntity> b)
    {
        b.ToTable("Ticket");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("Id").ValueGeneratedOnAdd();
        b.Property(x => x.Status).HasColumnName("Status").IsRequired();
        b.Property(x => x.VorstellungId).HasColumnName("VorstellungId").IsRequired();
        b.Property(x => x.SitzplatzId).HasColumnName("SitzplatzId").IsRequired();
        b.Property(x => x.UserId).HasColumnName("UserId");

        b.HasOne(x => x.User)
          .WithMany(k => k.Tickets)
          .HasForeignKey(x => x.UserId)
          .OnDelete(DeleteBehavior.SetNull);

        // unique seat per show
        b.HasIndex(x => new { x.VorstellungId, x.SitzplatzId }).IsUnique();
    }
}