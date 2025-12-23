using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KinoAppDB.Entities
{
    /// <summary>
    /// EF Core configuration for <see cref="UserEntity"/>.
    /// </summary>
    /// <remarks>
    /// Maps the user entity to the <c>User</c> table, enforces unique emails, and configures the one-to-many
    /// relationship between users and tickets.
    /// </remarks>
    public class KundeEntityConfig : IEntityTypeConfiguration<UserEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<UserEntity> b)
        {
            b.ToTable("User");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id)
             .HasColumnName("Id")
             .ValueGeneratedOnAdd();

            b.Property(x => x.Vorname)
             .HasColumnName("Vorname")
             .HasMaxLength(100)
             .IsRequired();

            b.Property(x => x.Nachname)
             .HasColumnName("Nachname")
             .HasMaxLength(100)
             .IsRequired();

            b.Property(x => x.Email)
             .HasColumnName("Email")
             .HasMaxLength(255)
             .IsRequired();

            b.Property(x => x.Passwort)
             .HasColumnName("Passwort")
             .HasMaxLength(255)
             .IsRequired();

            b.Property(x => x.Role)
             .HasColumnName("Role")
             .HasMaxLength(255)
             .IsRequired();

            b.HasIndex(x => x.Email)
             .IsUnique();

            b.HasMany(x => x.Tickets)
             .WithOne(x => x.User)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
