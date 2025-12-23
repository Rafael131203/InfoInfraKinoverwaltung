using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KinoAppDB.Entities
{
    public class SitzreiheEntityConfig : IEntityTypeConfiguration<SitzreiheEntity>
    {
        public void Configure(EntityTypeBuilder<SitzreiheEntity> b)
        {
            b.ToTable("Sitzreihe");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id).HasColumnName("Id").ValueGeneratedOnAdd();
            b.Property(x => x.Kategorie).HasColumnName("Kategorie").HasMaxLength(100).IsRequired();
            b.Property(x => x.Bezeichnung).HasColumnName("Bezeichnung").HasMaxLength(500).IsRequired();

            b.HasOne(s => s.Kinosaal)          // Sitzreihe -> one Kinosaal
             .WithMany(k => k.Sitzreihen)      // Kinosaal -> many Sitzreihen
             .HasForeignKey(s => s.KinosaalId) // FK property on Sitzreihe
             .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
