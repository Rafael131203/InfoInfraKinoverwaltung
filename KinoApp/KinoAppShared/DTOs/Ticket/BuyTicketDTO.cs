using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppShared.DTOs.Ticket
{
    public class BuyTicketDto
    {
        public long ShowId { get; set; }
        public int Amount { get; set; } // Wie viele Tickets?
        public decimal PricePerTicket { get; set; }
        // Evtl. noch SeatIds?
    }
}
