using KinoAppShared.DTOs;
using KinoAppShared.DTOs.Ticket;

namespace KinoAppWeb.Services
{
    public interface ITicketApiService
    {
        // Wir übergeben den Token optional mit, damit der Service den Header setzen kann
        Task BuyTicketAsync(BuyTicketDTO request, string? token);
        Task ReserveTicketsAsync(ReserveTicketDTO request, string? token);
    }
}