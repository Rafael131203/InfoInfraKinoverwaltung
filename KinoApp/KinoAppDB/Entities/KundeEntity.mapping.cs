using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KinoAppDB.Entities;

public class KundeEntityConfig : IEntityTypeConfiguration<KundeEntity>
{
    public void Configure(EntityTypeBuilder<KundeEntity> b)
    {
        b.ToTable("Kunde");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("Id").ValueGeneratedOnAdd();
        b.Property(x => x.Vorname).HasColumnName("Vorname").HasMaxLength(100).IsRequired();
        b.Property(x => x.Nachname).HasColumnName("Nachname").HasMaxLength(100).IsRequired();
        b.Property(x => x.Email).HasColumnName("Email").HasMaxLength(255).IsRequired();
        b.Property(x => x.Passwort).HasColumnName("Passwort").HasMaxLength(255).IsRequired();

        b.HasIndex(x => x.Email).IsUnique();

        // 1:1 optional cart; cart owns FK (Warenkorb has KundeId)
        b.HasOne(x => x.Warenkorb)
         .WithOne(x => x.Kunde)
         .HasForeignKey<WarenkorbEntity>(x => x.KundeId)
         .OnDelete(DeleteBehavior.Cascade);

        // tickets 1:n
        b.HasMany(x => x.Tickets)
         .WithOne(x => x.Kunde)
         .HasForeignKey(x => x.KundeId)
         .OnDelete(DeleteBehavior.SetNull);
    }
}