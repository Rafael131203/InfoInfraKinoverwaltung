using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppShared.DTOs
{
    public class TicketDTOs
    {
        public record SellTicketDto(Guid ShowId, int Quantity, decimal TotalPrice);
    }
}
