using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KinoAppDB.Entities
{
    /// <summary>
    /// EF Core configuration for <see cref="FilmEntity"/>.
    /// </summary>
    /// <remarks>
    /// Defines table mapping, column constraints, and the relationship between films and showings
    /// (<see cref="VorstellungEntity"/>).
    /// </remarks>
    public class FilmEntityConfig : IEntityTypeConfiguration<FilmEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<FilmEntity> builder)
        {
            builder.ToTable("Film");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.Id)
                   .HasColumnName("Id")
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(f => f.Titel)
                   .HasColumnName("Titel")
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(f => f.Beschreibung)
                   .HasColumnName("Beschreibung")
                   .HasMaxLength(4000);

            builder.Property(f => f.Dauer)
                   .HasColumnName("Dauer");

            builder.Property(f => f.Fsk)
                   .HasColumnName("FSK");

            builder.Property(f => f.Genre)
                   .HasColumnName("Genre")
                   .HasMaxLength(255);

            builder.Property(f => f.ImageURL)
                   .HasColumnName("ImageURL");

            builder.HasMany(f => f.Vorstellungen)
                   .WithOne(v => v.Film)
                   .HasForeignKey(v => v.FilmId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
