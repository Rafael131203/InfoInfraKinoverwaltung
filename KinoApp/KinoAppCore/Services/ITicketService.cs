using KinoAppShared.DTOs;

namespace KinoAppCore.Services // Namespace anpassen!
{
    public interface ITicketService
    {
        // 1. Return Type geändert: List<long> -> List<BuyTicketDTO>
        Task<List<BuyTicketDTO>> BuyTicketsAsync(BuyTicketDTO request, long? userId);

        // 2. Parameter geändert: long ticketId -> List<long> ticketIds
        Task CancelTicketsAsync(List<long> ticketIds);

        // 3. Neu hinzugefügt
        Task<List<BuyTicketDTO>> GetTicketsByUserIdAsync(long userId);

        Task<BuyTicketDTO> GetTicketAsync(long ticketId);
        Task<List<BuyTicketDTO>> GetAllAsync();
    }
}