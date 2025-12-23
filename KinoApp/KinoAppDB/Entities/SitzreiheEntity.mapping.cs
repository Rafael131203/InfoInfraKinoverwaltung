using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KinoAppDB.Entities
{
    /// <summary>
    /// EF Core configuration for <see cref="SitzreiheEntity"/>.
    /// </summary>
    /// <remarks>
    /// Maps the seat row entity to the <c>Sitzreihe</c> table and configures the relationship to
    /// <see cref="KinosaalEntity"/> and cascading delete behavior.
    /// </remarks>
    public class SitzreiheEntityConfig : IEntityTypeConfiguration<SitzreiheEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<SitzreiheEntity> b)
        {
            b.ToTable("Sitzreihe");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id)
             .HasColumnName("Id")
             .ValueGeneratedOnAdd();

            b.Property(x => x.Kategorie)
             .HasColumnName("Kategorie")
             .HasMaxLength(100)
             .IsRequired();

            b.Property(x => x.Bezeichnung)
             .HasColumnName("Bezeichnung")
             .HasMaxLength(500)
             .IsRequired();

            b.HasOne(s => s.Kinosaal)
             .WithMany(k => k.Sitzreihen)
             .HasForeignKey(s => s.KinosaalId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
