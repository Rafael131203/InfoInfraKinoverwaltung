using KinoAppCore.Services;
using KinoAppDB;
using KinoAppShared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KinoAppService.Controllers
{
    [Authorize] // Ensures only logged-in users (Admins or standard Users) can access this
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : BaseController
    {
        private readonly ITicketService _ticketService;

        // Inject IKinoAppDbContextScope and pass it to the base constructor
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

                    // Service call inside the transaction scope
                    var ticketIds = await _ticketService.BuyTicketsAsync(request, userId);

                    return Ok(new
                    {
                        Message = $"{ticketIds.Count} Ticket(s) erfolgreich gekauft!",
                        TicketIds = ticketIds
                    });
                }
                catch (InvalidOperationException ex)
                {
                    // Logic error (e.g. seat taken) - return 400
                    return BadRequest(ex.Message);
                }
                catch (ArgumentException ex)
                {
                    // Input error - return 400
                    return BadRequest(ex.Message);
                }
                catch (Exception ex)
                {
                    // Unexpected error - return 500
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
                    // Note: Usually you might want to check here if the ticket belongs to the current user
                    // or if the user is an Admin, unless the Service handles that logic.

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
                // Security check: Ensure user can only see their own tickets, unless they are Admin
                long? currentId = GetCurrentUserId();
                if (currentId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid(); // or Unauthorized()
                }

                var tickets = await _ticketService.GetTicketsByUserIdAsync(userId);
                return Ok(tickets);
            }, ct);

        // --- HILFSMETHODE ---
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