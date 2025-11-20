using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppCore.Services
{
    public interface IPreisService
    {
        Task<decimal> GetPreisAsync(SitzreihenKategorie kategorie, CancellationToken ct);
        Task SetPreisAsync(SitzreihenKategorie kategorie, decimal preis, CancellationToken ct);
    }

}
