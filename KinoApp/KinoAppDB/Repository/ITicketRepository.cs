using KinoAppDB.Entities;

namespace KinoAppDB.Repository
{
    // Erbt von IRepository<TicketEntity>, genau wie IUserRepository
    public interface ITicketRepository : IRepository<TicketEntity>
    {
        // Alle Tickets eines bestimmten Users finden (für "Meine Tickets")
        Task<List<TicketEntity>> GetTicketsByUserIdAsync(long userId, CancellationToken ct = default);

        // Alle Tickets einer Vorstellung finden (um Saalplan zu prüfen oder Details zu laden)
        Task<List<TicketEntity>> GetTicketsByVorstellungIdAsync(long vorstellungId, CancellationToken ct = default);

        // WICHTIG: Gibt nur die IDs der besetzten Plätze zurück.
        // Das ist performanter als ganze Entities zu laden, wenn wir nur prüfen wollen, was frei ist.
        Task<List<long>> GetBookedSeatIdsAsync(long vorstellungId, CancellationToken ct = default);

        Task<int> GetFreeSeatCountAsync(long vorstellungId, CancellationToken ct = default);

    }
}