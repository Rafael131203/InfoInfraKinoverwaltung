using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KinoAppDB.Entities
{
    /// <summary>
    /// EF Core configuration for <see cref="SitzplatzEntity"/>.
    /// </summary>
    /// <remarks>
    /// Maps the seat entity to the <c>Sitzplatz</c> table and configures the relationship to
    /// <see cref="SitzreiheEntity"/>.
    /// </remarks>
    public class SitzplatzEntityConfig : IEntityTypeConfiguration<SitzplatzEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<SitzplatzEntity> b)
        {
            b.ToTable("Sitzplatz");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id)
             .HasColumnName("Id")
             .ValueGeneratedOnAdd();

            b.Property(x => x.Nummer)
             .HasColumnName("Nummer")
             .IsRequired();

            b.Property(x => x.Preis)
             .HasColumnName("Preis")
             .HasColumnType("numeric(10,2)")
             .HasDefaultValue(0)
             .IsRequired();

            b.HasOne(x => x.Sitzreihe)
             .WithMany(x => x.Sitzplätze)
             .HasForeignKey(x => x.SitzreiheId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
