using KinoAppShared.DTOs;

namespace KinoAppWeb.Services
{
    public interface ITicketApiService
    {
        // Wir übergeben den Token optional mit, damit der Service den Header setzen kann
        Task BuyTicketAsync(BuyTicketDTO request, string? token);
    }
}