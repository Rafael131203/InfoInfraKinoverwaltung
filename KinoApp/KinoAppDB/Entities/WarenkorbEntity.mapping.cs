using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KinoAppDB.Entities;

public class WarenkorbEntityConfig : IEntityTypeConfiguration<WarenkorbEntity>
{
    public void Configure(EntityTypeBuilder<WarenkorbEntity> b)
    {
        b.ToTable("Warenkorb");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("Id").ValueGeneratedOnAdd();
        b.Property(x => x.Gesamtpreis).HasColumnName("Gesamtpreis").HasColumnType("numeric(10,2)").HasDefaultValue(0).IsRequired();
        b.Property(x => x.Zahlungsmittel).HasColumnName("Zahlungsmittel");

        b.Property(x => x.KundeId).HasColumnName("Kunde_Id");
        b.HasIndex(x => x.KundeId).IsUnique(); // each Kunde has at most one cart
    }
}