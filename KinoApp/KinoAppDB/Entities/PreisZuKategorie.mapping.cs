using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KinoAppDB.Entities
{
    public class PreisZuKategorieEntityConfig : IEntityTypeConfiguration<PreisZuKategorieEntity>
    {
        public void Configure(EntityTypeBuilder<PreisZuKategorieEntity> b)
        {
            b.ToTable("PreisZuKategorie");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id).HasColumnName("Id").ValueGeneratedOnAdd();
            b.Property(x => x.Kategorie).HasColumnName("Kategorie").IsRequired();
            b.Property(x => x.Preis).HasColumnName("Preis").IsRequired();
        }
    }
}
