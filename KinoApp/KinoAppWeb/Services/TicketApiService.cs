using KinoAppShared.DTOs;
using KinoAppShared.DTOs.Ticket;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace KinoAppWeb.Services
{
    public class TicketApiService : ITicketApiService
    {
        private readonly HttpClient _http;

        public TicketApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task BuyTicketAsync(BuyTicketDTO request, string? token)
        {
            // 1. Header setzen (Logik wie besprochen: Token oder nix)
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _http.DefaultRequestHeaders.Authorization = null;
            }

            // 2. Request senden
            // (Pfad muss exakt zu deinem Backend Controller passen: api/Tickets/buy)
            var response = await _http.PostAsJsonAsync("api/Tickets/buy", request);

            // 3. Fehlerbehandlung (Stil deines Kollegen)
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Fehler beim Kauf: {error}");
            }
        }

        public async Task ReserveTicketsAsync(ReserveTicketDTO request, string? token)
        {
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            else
                _http.DefaultRequestHeaders.Authorization = null;

            var response = await _http.PostAsJsonAsync("api/Tickets/reserve", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Fehler bei der Reservierung: {error}");
            }
        }
    }
}