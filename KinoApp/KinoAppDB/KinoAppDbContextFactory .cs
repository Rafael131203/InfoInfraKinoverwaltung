// KinoAppDB/KinoAppDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KinoAppDB;
public sealed class KinoAppDbContextFactory : IDesignTimeDbContextFactory<KinoAppDbContext>
{
    public KinoAppDbContext CreateDbContext(string[] args)
    {
        var opts = new DbContextOptionsBuilder<KinoAppDbContext>()
            // Use your provider here (Postgres example):
            .UseNpgsql("Host=localhost;Database=kinoapp_dev;Username=postgres;Password=postgres")
            .Options;

        return new KinoAppDbContext(opts);
    }
}
