using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KinoAppDB.Entities;

public class KundeEntityConfig : IEntityTypeConfiguration<KundeEntity>
{
    public void Configure(EntityTypeBuilder<KundeEntity> b)
    {
        b.ToTable("kunde");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        b.Property(x => x.Vorname).HasColumnName("vorname").HasMaxLength(100).IsRequired();
        b.Property(x => x.Nachname).HasColumnName("nachname").HasMaxLength(100).IsRequired();
        b.Property(x => x.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        b.Property(x => x.PasswortHash).HasColumnName("passwort_hash").HasMaxLength(255).IsRequired();

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