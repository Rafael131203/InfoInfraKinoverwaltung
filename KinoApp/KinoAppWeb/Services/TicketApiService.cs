using System.Net.Http.Headers;
using System.Net.Http.Json;
using KinoAppShared.DTOs;
using KinoAppShared.DTOs.Ticket;

namespace KinoAppWeb.Services
{
    /// <summary>
    /// Client-side API wrapper for ticket purchase and reservation endpoints.
    /// </summary>
    /// <remarks>
    /// The service supports both authenticated and anonymous calls by optionally applying a bearer token.
    /// </remarks>
    public class TicketApiService : ITicketApiService
    {
        private readonly HttpClient _http;

        /// <summary>
        /// Creates a new <see cref="TicketApiService"/>.
        /// </summary>
        /// <param name="http">HTTP client configured with the backend base address.</param>
        public TicketApiService(HttpClient http)
        {
            _http = http;
        }

        /// <summary>
        /// Purchases one or more tickets for a given showtime and seat selection.
        /// </summary>
        /// <param name="request">Ticket purchase request payload.</param>
        /// <param name="token">Optional bearer token. If null/empty, the call is made anonymously.</param>
        /// <exception cref="InvalidOperationException">Thrown when the backend returns a non-success status.</exception>
        public async Task BuyTicketAsync(BuyTicketDTO request, string? token)
        {
            ApplyBearerToken(token);

            var response = await _http.PostAsJsonAsync("api/Tickets/buy", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Fehler beim Kauf: {error}");
            }
        }

        /// <summary>
        /// Reserves one or more tickets for a given showtime and seat selection.
        /// </summary>
        /// <param name="request">Reservation request payload.</param>
        /// <param name="token">Optional bearer token. If null/empty, the call is made anonymously.</param>
        /// <exception cref="InvalidOperationException">Thrown when the backend returns a non-success status.</exception>
        public async Task ReserveTicketsAsync(ReserveTicketDTO request, string? token)
        {
            ApplyBearerToken(token);

            var response = await _http.PostAsJsonAsync("api/Tickets/reserve", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Fehler bei der Reservierung: {error}");
            }
        }

        /// <summary>
        /// Applies (or clears) the Authorization header on the underlying HttpClient.
        /// </summary>
        private void ApplyBearerToken(string? token)
        {
            if (!string.IsNullOrWhiteSpace(token))
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            else
                _http.DefaultRequestHeaders.Authorization = null;
        }
    }
}
