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
            // 1. Scope/Transaktion starten
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

                // Fallback
                decimal ticketPreis = sitzplatz != null ? sitzplatz.Preis : request.PreisVorschlag;

                // 4. DB-ENTITY ERSTELLEN
                var dbTicket = new TicketEntity
                {
                    VorstellungId = request.VorstellungId,
                    SitzplatzId = request.SitzplatzId,
                    Status = 1,
                    KundeId = kundenId
                };

                // 5. SPEICHERN
                await _ticketRepo.AddAsync(dbTicket);
                await _ticketRepo.SaveAsync(); // Generiert ID

                // 6. EVENT SENDEN
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
                return StatusCode(500, $"Fehler beim Ticketkauf: {ex.Message}");
            }
        }

        [HttpPost("cancel/{id}")]
        public async Task<IActionResult> CancelTicket(long id)
        {
            await _dbScope.BeginAsync();
            try
            {
                // 1. Ticket laden (inkl. Sitzplatz für den Preis!)
                // Wir brauchen den Preis, um die Statistik zu korrigieren.
                // Dazu müssen wir "Include" nutzen, oder den Sitzplatz separat laden.
                var ticket = await _ticketRepo.GetByIdAsync(id);

                if (ticket == null) return NotFound("Ticket nicht gefunden.");
                if (ticket.Status == 2) return BadRequest("Ticket ist bereits storniert.");

                // Preis ermitteln (für die Rückerstattung/Statistik)
                var sitzplatz = await _sitzplatzRepo.GetByIdAsync(ticket.SitzplatzId);
                decimal erstattung = sitzplatz?.Preis ?? 0;

                // 2. Status ändern (2 = Storniert)
                ticket.Status = 2;

                // Update & Save
                await _ticketRepo.UpdateAsync(ticket);
                await _ticketRepo.SaveAsync();

                // 3. Event senden (Minus-Geschäft für die Statistik)
                await _bus.PublishAsync(new TicketCancelled(
                    ticket.Id,
                    ticket.VorstellungId,
                    erstattung,
                    DateTime.UtcNow
                ));

                // Transaktion bestätigen
                await _dbScope.CommitAsync();

                return Ok(new { Message = "Ticket erfolgreich storniert." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
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