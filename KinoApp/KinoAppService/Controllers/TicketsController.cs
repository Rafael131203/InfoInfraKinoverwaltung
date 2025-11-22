using KinoAppCore.Abstractions;
using KinoAppDB;
using KinoAppDB.Entities;       // NUR eure DB-Entities
using KinoAppDB.Repository;
using KinoAppShared.DTOs;
using KinoAppShared.Messaging;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KinoAppService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        // Wir arbeiten direkt mit der TicketEntity
        private readonly IRepository<TicketEntity> _ticketRepo;
        private readonly IRepository<SitzplatzEntity> _sitzplatzRepo;
        private readonly IMessageBus _bus;
        private readonly IKinoAppDbContextScope _dbScope;

        public TicketsController(
            IRepository<TicketEntity> ticketRepo,
            IRepository<SitzplatzEntity> sitzplatzRepo,
            IMessageBus bus,
            IKinoAppDbContextScope dbScope)
        {
            _ticketRepo = ticketRepo;
            _sitzplatzRepo = sitzplatzRepo;
            _bus = bus;
            _dbScope = dbScope;
        }

        [HttpPost("buy")]
        public async Task<IActionResult> BuyTicket([FromBody] BuyTicketDTO request)
        {
            // 1. Transaktion starten
            await _dbScope.BeginAsync();

            try
            {
                // 2. USER IDENTIFIZIEREN (JWT vs. Gast)
                long? kundenId = GetCurrentUserId();

                // Validierung für Gast
                if (kundenId == null && string.IsNullOrWhiteSpace(request.GastEmail))
                {
                    return BadRequest("Als Gast müssen Sie eine E-Mail-Adresse angeben.");
                }

                // 3. PREIS LADEN (Aus Sitzplatz-Tabelle)
                var sitzplatz = await _sitzplatzRepo.GetByIdAsync(request.SitzplatzId);

                // Fallback, falls DB leer ist (damit wir testen können):
                decimal ticketPreis = sitzplatz != null ? sitzplatz.Preis : request.PreisVorschlag;

                // 4. DB-ENTITY ERSTELLEN (Ohne Warenkorb!)
                var dbTicket = new TicketEntity
                {
                    VorstellungId = request.VorstellungId,
                    SitzplatzId = request.SitzplatzId,
                    Status = 1,
                    KundeId = kundenId
                };

                // 5. SPEICHERN IN TRANS
                await _ticketRepo.AddAsync(dbTicket);
                await _ticketRepo.SaveAsync(); // Generiert ID

                // 6. EVENT SENDEN (Für MongoDB)
                await _bus.PublishAsync(new TicketSold(
                    dbTicket.Id,
                    request.VorstellungId,
                    request.Anzahl,
                    request.Anzahl * ticketPreis,
                    DateTime.UtcNow
                ));

                // 7. COMMIT 
                await _dbScope.CommitAsync(); 

                return Ok(new
                {
                    Message = "Ticket erfolgreich gekauft!",
                    TicketId = dbTicket.Id,
                    KundeId = kundenId
                });
            }
            catch (Exception ex)
            {
                // Optional: Fehler loggen
                return StatusCode(500, $"Fehler beim Ticketkauf: {ex.Message}");
            }
        }

        // Hilfsmethode für JWT
        private long? GetCurrentUserId()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated) return null;

            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                        ?? User.FindFirst("id")
                        ?? User.FindFirst("sub");

            if (idClaim != null && long.TryParse(idClaim.Value, out long id)) return id;

            return null;
        }
    }
}