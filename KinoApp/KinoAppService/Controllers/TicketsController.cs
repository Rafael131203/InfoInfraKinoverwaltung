using KinoAppCore.Services;
using KinoAppDB;
using KinoAppShared.DTOs;
using KinoAppShared.DTOs.Ticket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KinoAppService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : BaseController
    {
        private readonly ITicketService _ticketService;

        public TicketsController(ITicketService ticketService, IKinoAppDbContextScope scope)
            : base(scope)
        {
            _ticketService = ticketService;
        }

        // POST: api/tickets/buy
        [HttpPost("buy")]
        [AllowAnonymous]
        public Task<IActionResult> BuyTickets([FromBody] BuyTicketDTO request, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                try
                {
                    long? userId = GetCurrentUserId();

                    var tickets = await _ticketService.BuyTicketsAsync(request, userId);

                    return Ok(new
                    {
                        Message = $"{tickets.Count} Ticket(s) erfolgreich gekauft!",
                        Tickets = tickets
                    });
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(ex.Message);
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(ex.Message);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Interner Fehler: {ex.Message}");
                }
            }, ct);

        // 🔥 NEW: POST api/tickets/reserve
        [HttpPost("reserve")]
        [AllowAnonymous]
        public Task<IActionResult> ReserveTickets([FromBody] ReserveTicketDTO request, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                try
                {
                    long? userId = GetCurrentUserId();

                    await _ticketService.ReserveTicketsAsync(request, userId, token);

                    return Ok(new
                    {
                        Message = $"{request.SitzplatzIds.Count} Ticket(s) erfolgreich reserviert!"
                    });
                }
                catch (InvalidOperationException ex)
                {
                    // e.g. already reserved / booked
                    return BadRequest(ex.Message);
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(ex.Message);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Interner Fehler: {ex.Message}");
                }
            }, ct);

        // POST: api/tickets/cancel/{id}
        [HttpPost("cancel/{id}")]
        public Task<IActionResult> CancelTicket(long id, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                try
                {
                    await _ticketService.CancelTicketsAsync(new List<long> { id });

                    return Ok(new { Message = "Ticket erfolgreich storniert." });
                }
                catch (KeyNotFoundException ex)
                {
                    return NotFound(ex.Message);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Fehler beim Stornieren: {ex.Message}");
                }
            }, ct);

        // GET: api/tickets/user/{userId}
        [HttpGet("user/{userId}")]
        public Task<IActionResult> GetTicketsByUser(long userId, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                long? currentId = GetCurrentUserId();
                if (currentId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var tickets = await _ticketService.GetTicketsByUserIdAsync(userId);
                return Ok(tickets);
            }, ct);

        private long? GetCurrentUserId()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
                return null;

            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                          ?? User.FindFirst("id")
                          ?? User.FindFirst("sub");

            if (idClaim != null && long.TryParse(idClaim.Value, out long id))
                return id;

            return null;
        }
    }
}
