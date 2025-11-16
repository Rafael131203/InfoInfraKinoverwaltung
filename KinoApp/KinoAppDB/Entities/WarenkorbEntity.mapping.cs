using KinoAppDB.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KinoAppDB.Config;

public class WarenkorbEntityConfig : IEntityTypeConfiguration<WarenkorbEntity>
{
    public void Configure(EntityTypeBuilder<WarenkorbEntity> b)
    {
        b.ToTable("warenkorb");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        b.Property(x => x.Gesamtpreis).HasColumnName("gesamtpreis").HasColumnType("numeric(10,2)").HasDefaultValue(0).IsRequired();
        b.Property(x => x.Zahlungsmittel).HasColumnName("zahlungsmittel");

        b.Property(x => x.KundeId).HasColumnName("kunde_id");
        b.HasIndex(x => x.KundeId).IsUnique(); // each Kunde has at most one cart
    }
}