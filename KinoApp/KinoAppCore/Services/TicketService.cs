using AutoMapper;
using KinoAppCore.Abstractions;
using KinoAppDB.Entities;
using KinoAppDB.Repository;
using KinoAppShared.DTOs;
using KinoAppShared.DTOs.Ticket;
using KinoAppShared.Enums;
using KinoAppShared.Messaging;
using Microsoft.EntityFrameworkCore;


namespace KinoAppCore.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepo;
        private readonly ISitzplatzRepository _sitzplatzRepo;
        private readonly IMessageBus _messageBus;
        private readonly IMapper _mapper;

        public TicketService(
            ITicketRepository ticketRepo,
            ISitzplatzRepository sitzplatzRepo,
            IMessageBus messageBus,
            IMapper mapper)
        {
            _ticketRepo = ticketRepo;
            _sitzplatzRepo = sitzplatzRepo;
            _messageBus = messageBus;
            _mapper = mapper;
        }

        // ---------------- BUY = from Reserved -> Booked ----------------
        public async Task<List<BuyTicketDTO>> BuyTicketsAsync(BuyTicketDTO request, long? userId)
        {
            // Load tickets for this Vorstellung + seats
            var tickets = await _ticketRepo.Query(false)
                .Where(t => t.VorstellungId == request.VorstellungId &&
                            request.SitzplatzIds.Contains(t.SitzplatzId))
                .Include(t => t.Sitzplatz)
                .ToListAsync();

            // Ensure all requested seats exist
            if (tickets.Count != request.SitzplatzIds.Count)
                throw new ArgumentException("Einer oder mehrere Sitzplätze sind für diese Vorstellung nicht vorhanden.");

            // All must currently be RESERVED
            if (tickets.Any(t => t.Status != (int)TicketStatus.Reserved))
                throw new InvalidOperationException("Einer der gewählten Plätze ist nicht (mehr) reserviert.");

            // Optional: ensure the same logged-in user is buying their own reservation
            if (userId.HasValue && tickets.Any(t => t.UserId != userId))
                throw new InvalidOperationException("Einer der gewählten Plätze ist nicht mehr für Sie reserviert.");

            // Book them
            foreach (var ticket in tickets)
            {
                ticket.Status = (int)TicketStatus.Booked;
                ticket.UserId = userId;  // keep/assign owner
            }

            await _ticketRepo.SaveAsync();

            // Pricing
            var gesamtPreis = tickets.Sum(t => t.Sitzplatz?.Preis ?? 0m);

            // Event senden (z.B. erste TicketId als Ref)
            await _messageBus.PublishAsync(new TicketSold(
                tickets[0].Id,
                request.VorstellungId,
                tickets.Count,
                gesamtPreis,
                DateTime.UtcNow
            ));

            return _mapper.Map<List<BuyTicketDTO>>(tickets);
        }

        // ---------------- RESERVE = from Free -> Reserved ---------------
        public async Task ReserveTicketsAsync(ReserveTicketDTO request, long? userId, CancellationToken ct)
        {
            // Load tickets for this Vorstellung + seats
            var tickets = await _ticketRepo.Query(false)
                .Where(t => t.VorstellungId == request.VorstellungId &&
                            request.SitzplatzIds.Contains(t.SitzplatzId))
                .ToListAsync(ct);

            if (tickets.Count != request.SitzplatzIds.Count)
                throw new ArgumentException("Einer oder mehrere Sitzplätze sind für diese Vorstellung nicht vorhanden.");

            // Only FREE seats can be reserved
            if (tickets.Any(t => t.Status != (int)TicketStatus.Free))
                throw new InvalidOperationException("Einer der gewählten Plätze ist bereits reserviert oder gebucht.");

            foreach (var ticket in tickets)
            {
                ticket.Status = (int)TicketStatus.Reserved;
                ticket.UserId = userId; // null for guests, or actual user if logged-in
            }

            await _ticketRepo.SaveAsync(ct);
        }

        // ---------------- CANCEL = back to Free -------------------------
        public async Task CancelTicketsAsync(List<long> ticketIds)
        {
            var ticketsToCancel = await _ticketRepo.Query(false)
                .Where(t => ticketIds.Contains(t.Id))
                .Include(t => t.Sitzplatz)
                .ToListAsync();

            foreach (var ticket in ticketsToCancel)
            {
                // mark as free again
                ticket.Status = (int)TicketStatus.Free;
                ticket.UserId = null;

                await _messageBus.PublishAsync(new TicketCancelled(
                    ticket.Id,
                    ticket.VorstellungId,
                    ticket.Sitzplatz?.Preis ?? 0,
                    DateTime.UtcNow
                ));
            }

            await _ticketRepo.SaveAsync();
        }

        public async Task<BuyTicketDTO> GetTicketAsync(long ticketId)
        {
            var ticket = await _ticketRepo.GetByIdAsync(ticketId);
            if (ticket == null) return null;

            return _mapper.Map<BuyTicketDTO>(ticket);
        }

        public async Task<List<BuyTicketDTO>> GetTicketsByUserIdAsync(long userId)
        {
            var userTickets = await _ticketRepo.GetTicketsByUserIdAsync(userId);
            return _mapper.Map<List<BuyTicketDTO>>(userTickets);
        }

        public async Task<List<BuyTicketDTO>> GetAllAsync()
        {
            var allTickets = await _ticketRepo.Query().ToListAsync();
            return _mapper.Map<List<BuyTicketDTO>>(allTickets);
        }

        private static TicketStatus ParseStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status darf nicht leer sein.");

            return status.Trim().ToLower() switch
            {
                "nothing" => TicketStatus.Nothing,
                "free" => TicketStatus.Free,
                "reserved" => TicketStatus.Reserved,
                "booked" => TicketStatus.Booked,
                _ => throw new ArgumentException($"Unbekannter Ticket-Status: {status}")
            };
        }

        public async Task CreateTicketsForVorstellungAsync(long vorstellungId, long? kinosaalId, CancellationToken ct)
        {
            var sitzplaetze = await _sitzplatzRepo.Query()
                .Include(s => s.Sitzreihe)
                .Where(s => s.Sitzreihe.KinosaalId == kinosaalId)
                .ToListAsync(ct);

            var tickets = new List<TicketEntity>();

            foreach (var platz in sitzplaetze)
            {
                var ticket = new TicketEntity
                {
                    VorstellungId = vorstellungId,
                    SitzplatzId = platz.Id,
                    UserId = null,
                    Status = (int)TicketStatus.Free
                };

                tickets.Add(ticket);
                await _ticketRepo.AddAsync(ticket, ct);
            }

            await _ticketRepo.SaveAsync(ct);
        }

        public Task<int> GetFreeSeatCountAsync(long vorstellungId, CancellationToken ct)
        {
            return _ticketRepo.GetFreeSeatCountAsync(vorstellungId, ct);
        }

        public async Task UpdateTicketStatusAsync(UpdateTicketStatusDTO dto, CancellationToken ct)
        {
            var ticket = await _ticketRepo.Query(false)
                .FirstOrDefaultAsync(t => t.Id == dto.TicketId, ct);

            if (ticket == null)
                throw new KeyNotFoundException($"Ticket mit Id {dto.TicketId} nicht gefunden.");

            var status = ParseStatus(dto.Status);
            ticket.Status = (int)status;
            ticket.UserId = dto.UserId;

            await _ticketRepo.SaveAsync(ct);
        }
    }
}
