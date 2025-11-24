using AutoMapper;
using KinoAppCore.Abstractions;
using KinoAppDB.Entities;
using KinoAppDB.Repository;
using KinoAppShared.DTOs;
using KinoAppShared.Messaging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        // --------------- 1. Change return to DTO ---------------
        public async Task<List<BuyTicketDTO>> BuyTicketsAsync(BuyTicketDTO request, long? userId)
        {
            // A. Prüfen
            var gebuchtePlatzIds = await _ticketRepo.GetBookedSeatIdsAsync(request.VorstellungId);
            if (request.SitzplatzIds.Any(id => gebuchtePlatzIds.Contains(id)))
            {
                throw new InvalidOperationException($"Einer der gewählten Plätze ist bereits vergeben.");
            }

            // B. Preise laden
            var sitzplaetze = await _sitzplatzRepo.Query()
                .Where(s => request.SitzplatzIds.Contains(s.Id))
                .ToListAsync();

            if (sitzplaetze.Count != request.SitzplatzIds.Count)
                throw new ArgumentException("Ungültige Sitzplatz-IDs übermittelt.");

            // C. Erstellen
            var neueTickets = new List<TicketEntity>();

            foreach (var platz in sitzplaetze)
            {
                var ticket = new TicketEntity
                {
                    VorstellungId = request.VorstellungId,
                    SitzplatzId = platz.Id,
                    UserId = userId,
                    Status = 1
                };

                neueTickets.Add(ticket);
                await _ticketRepo.AddAsync(ticket);
            }

            await _ticketRepo.SaveAsync();

            // D. Event senden
            var gesamtPreis = sitzplaetze.Sum(s => s.Preis);

            await _messageBus.PublishAsync(new TicketSold(
                neueTickets[0].Id,       // TicketId (Wir nehmen die erste ID als Referenz)
                request.VorstellungId,   // ShowId
                neueTickets.Count,       // Quantity
                gesamtPreis,             // TotalPrice
                DateTime.UtcNow          // SoldAtUtc
            ));

            // UMSETZUNG KOMMENTAR: Return as DTO (via Mapper)
            return _mapper.Map<List<BuyTicketDTO>>(neueTickets);
        }

        // --------------- 2. Loop for refund change to list ---------------
        // Signatur geändert von (long ticketId) zu (List<long> ticketIds)
        public async Task CancelTicketsAsync(List<long> ticketIds)
        {
            // Wir laden alle betroffenen Tickets inkl. Sitzplatz (für den Preis)
            var ticketsToCancel = await _ticketRepo.Query()
                .Where(t => ticketIds.Contains(t.Id))
                .Include(t => t.Sitzplatz)
                .ToListAsync();

            // UMSETZUNG KOMMENTAR: Loop for refund
            foreach (var ticket in ticketsToCancel)
            {
                // 1. Zum Löschen markieren
                await _ticketRepo.DeleteAsync(ticket);

                // 2. Event senden (pro Ticket, damit die Statistik stimmt)
                await _messageBus.PublishAsync(new TicketCancelled(
                    ticket.Id,                      // 1. TicketId
                    ticket.VorstellungId,           // 2. ShowId
                    ticket.Sitzplatz?.Preis ?? 0,   // 3. AmountToRefund (Preis oder 0 falls null)
                    DateTime.UtcNow                 // 4. CancelledAtUtc
            ));
            }

            // Am Ende einmal speichern für alle Löschvorgänge
            await _ticketRepo.SaveAsync();
        }

        public async Task<BuyTicketDTO> GetTicketAsync(long ticketId)
        {
            var ticket = await _ticketRepo.GetByIdAsync(ticketId);
            if (ticket == null) return null;

            return _mapper.Map<BuyTicketDTO>(ticket);
        }

        // --------------- 3. Add get ticket with userid ---------------
        public async Task<List<BuyTicketDTO>> GetTicketsByUserIdAsync(long userId)
        {
            // Holt die Tickets aus der DB (Repo Methode haben wir vorher erstellt)
            var userTickets = await _ticketRepo.GetTicketsByUserIdAsync(userId);

            // UMSETZUNG KOMMENTAR: Loop logic (wird hier vom Mapper übernommen, der durch die Liste iteriert)
            return _mapper.Map<List<BuyTicketDTO>>(userTickets);
        }

        public async Task<List<BuyTicketDTO>> GetAllAsync()
        {
            var allTickets = await _ticketRepo.Query().ToListAsync();
            return _mapper.Map<List<BuyTicketDTO>>(allTickets);
        }
    }
}