using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppDB.Entities
{
    public class SitzplatzEntityConfig : IEntityTypeConfiguration<SitzplatzEntity>
    {
        public void Configure(EntityTypeBuilder<SitzplatzEntity> b)
        {
            b.ToTable("Sitzplatz");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id).HasColumnName("Id").ValueGeneratedOnAdd();
            b.Property(x => x.Gebucht).HasColumnName("Gebucht").IsRequired();
            b.Property(x => x.Nummer).HasColumnName("Nummer").IsRequired();
            b.Property(x => x.Preis).HasColumnName("Preis").HasColumnType("numeric(10,2)").HasDefaultValue(0).IsRequired();

            b.HasOne(x => x.Sitzreihe)
             .WithMany(x => x.Sitzplätze)
             .HasForeignKey(x => x.SitzreiheId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
