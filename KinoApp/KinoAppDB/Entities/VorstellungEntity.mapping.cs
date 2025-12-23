using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KinoAppDB.Entities
{
    /// <summary>
    /// EF Core configuration for <see cref="VorstellungEntity"/>.
    /// </summary>
    /// <remarks>
    /// Configures table/column mappings and relationships to <see cref="FilmEntity"/> and <see cref="KinosaalEntity"/>.
    /// </remarks>
    public class VorstellungEntityConfig : IEntityTypeConfiguration<VorstellungEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<VorstellungEntity> b)
        {
            b.ToTable("Vorstellung");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id)
             .HasColumnName("Id")
             .ValueGeneratedOnAdd();

            b.Property(x => x.Datum)
             .HasColumnName("Datum")
             .IsRequired();

            b.Property(x => x.Status)
             .HasColumnName("Status")
             .HasConversion<int>()
             .IsRequired();

            b.Property(x => x.FilmId)
             .HasColumnName("FilmId");

            b.Property(x => x.KinosaalId)
             .HasColumnName("KinosaalId");

            b.HasOne(x => x.Film)
             .WithMany(f => f.Vorstellungen)
             .HasForeignKey(x => x.FilmId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Kinosaal)
             .WithMany(w => w.Vorstellungen)
             .HasForeignKey(x => x.KinosaalId)
             .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
