using KinoAppCore.Services;
using KinoAppDB;
using KinoAppShared.DTOs;
using KinoAppShared.DTOs.Ticket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KinoAppService.Controllers
{
    /// <summary>
    /// API endpoints for reserving, buying, canceling, and listing tickets.
    /// </summary>
    /// <remarks>
    /// The controller supports both authenticated users and guests. When a user is authenticated,
    /// operations may be associated with a user identifier derived from JWT claims.
    /// </remarks>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : BaseController
    {
        private readonly ITicketService _ticketService;

        /// <summary>
        /// Creates a new <see cref="TicketsController"/>.
        /// </summary>
        /// <param name="ticketService">Ticket service handling business logic.</param>
        /// <param name="scope">Database scope used for transactional execution.</param>
        public TicketsController(ITicketService ticketService, IKinoAppDbContextScope scope)
            : base(scope)
        {
            _ticketService = ticketService;
        }

        /// <summary>
        /// Purchases previously reserved tickets.
        /// </summary>
        /// <param name="request">Purchase request containing showing and seat IDs.</param>
        /// <param name="ct">Cancellation token.</param>
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

        /// <summary>
        /// Reserves free seats for a showing.
        /// </summary>
        /// <param name="request">Reservation request.</param>
        /// <param name="ct">Cancellation token.</param>
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

        /// <summary>
        /// Cancels a ticket and returns it to the free pool.
        /// </summary>
        /// <param name="id">Ticket identifier.</param>
        /// <param name="ct">Cancellation token.</param>
        [HttpPost("cancel/{id}")]
        [AllowAnonymous]
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

        /// <summary>
        /// Returns tickets for a specific user.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <remarks>
        /// Users can only request their own tickets unless they are in the <c>Admin</c> role.
        /// </remarks>
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "User")]
        public Task<IActionResult> GetTicketsByUser(long userId, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                long? currentId = GetCurrentUserId();
                if (currentId != userId && !User.IsInRole("Admin"))
                    return Forbid();

                var tickets = await _ticketService.GetTicketsByUserIdAsync(userId);
                return Ok(tickets);
            }, ct);

        /// <summary>
        /// Extracts the current user identifier from common JWT claim types.
        /// </summary>
        /// <returns>The user id if available; otherwise <c>null</c> for guests.</returns>
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
