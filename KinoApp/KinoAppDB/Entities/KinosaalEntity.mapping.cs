using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KinoAppDB.Entities
{
    /// <summary>
    /// EF Core configuration for <see cref="KinosaalEntity"/>.
    /// </summary>
    /// <remarks>
    /// Maps the auditorium entity to the <c>Kinosaal</c> table and configures key generation and
    /// required fields.
    /// </remarks>
    public class KinosaalEntityConfig : IEntityTypeConfiguration<KinosaalEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<KinosaalEntity> b)
        {
            b.ToTable("Kinosaal");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id)
             .HasColumnName("Id")
             .ValueGeneratedOnAdd();

            b.Property(x => x.Name)
             .HasColumnName("Name")
             .IsRequired();
        }
    }
}
