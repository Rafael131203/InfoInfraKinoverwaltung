using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KinoAppDB.Entities
{
    public class KinosaalEntityConfig : IEntityTypeConfiguration<KinosaalEntity>
    {
        public void Configure(EntityTypeBuilder<KinosaalEntity> b)
        {
            b.ToTable("Kinosaal");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id).HasColumnName("Id").ValueGeneratedOnAdd();
            b.Property(x => x.Name).HasColumnName("Name").IsRequired();
        }
    }
}
