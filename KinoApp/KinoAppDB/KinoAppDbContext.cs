using Microsoft.EntityFrameworkCore;

namespace KinoAppDB
{
    /// <summary>
    /// EF Core database context for the KinoApp application.
    /// </summary>
    /// <remarks>
    /// This context aggregates all relational entities and applies configuration classes
    /// using EF Core's assembly scanning mechanism.
    /// </remarks>
    public class KinoAppDbContext : DbContext
    {
        /// <summary>
        /// Creates a new <see cref="KinoAppDbContext"/>.
        /// </summary>
        /// <param name="options">EF Core context options.</param>
        public KinoAppDbContext(DbContextOptions<KinoAppDbContext> options) : base(options) { }

        /// <summary>
        /// Users registered in the system.
        /// </summary>
        public DbSet<Entities.UserEntity> Kunden => Set<Entities.UserEntity>();

        /// <summary>
        /// Tickets for all showings and seats.
        /// </summary>
        public DbSet<Entities.TicketEntity> Tickets => Set<Entities.TicketEntity>();

        /// <summary>
        /// Auditoriums available in the cinema.
        /// </summary>
        public DbSet<Entities.KinosaalEntity> Kinosaal => Set<Entities.KinosaalEntity>();

        /// <summary>
        /// Seat rows belonging to auditoriums.
        /// </summary>
        public DbSet<Entities.SitzreiheEntity> Sitzreihe => Set<Entities.SitzreiheEntity>();

        /// <summary>
        /// Individual seats.
        /// </summary>
        public DbSet<Entities.SitzplatzEntity> Sitzplatz => Set<Entities.SitzplatzEntity>();

        /// <summary>
        /// Films available for scheduling.
        /// </summary>
        public DbSet<Entities.FilmEntity> Film => Set<Entities.FilmEntity>();

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(KinoAppDbContext).Assembly);
        }
    }
}
