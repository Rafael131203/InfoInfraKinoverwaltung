using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KinoAppDB.Entities
{
    /// <summary>
    /// EF Core configuration for <see cref="PreisZuKategorieEntity"/>.
    /// </summary>
    /// <remarks>
    /// Maps category-based seat pricing to the <c>PreisZuKategorie</c> table.
    /// </remarks>
    public class PreisZuKategorieEntityConfig : IEntityTypeConfiguration<PreisZuKategorieEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<PreisZuKategorieEntity> b)
        {
            b.ToTable("PreisZuKategorie");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id)
             .HasColumnName("Id")
             .ValueGeneratedOnAdd();

            b.Property(x => x.Kategorie)
             .HasColumnName("Kategorie")
             .IsRequired();

            b.Property(x => x.Preis)
             .HasColumnName("Preis")
             .IsRequired();
        }
    }
}
