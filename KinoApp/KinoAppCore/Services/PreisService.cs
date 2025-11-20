using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppCore.Services
{
    public class PreisService : IPreisService
    {
        private readonly Dictionary<SitzreihenKategorie, decimal> _preise = new()
    {
        { SitzreihenKategorie.Parkett, 10.00m },
        { SitzreihenKategorie.LOGE, 15.00m },
        { SitzreihenKategorie.LOGEPLUS, 20.00m }
    };

        public Task<decimal> GetPreisAsync(SitzreihenKategorie kategorie, CancellationToken ct)
        {
            if (!_preise.ContainsKey(kategorie))
                throw new ArgumentException("Kategorie nicht gefunden");

            return Task.FromResult(_preise[kategorie]);
        }

        public Task SetPreisAsync(SitzreihenKategorie kategorie, decimal preis, CancellationToken ct)
        {
            if (!_preise.ContainsKey(kategorie))
                throw new ArgumentException("Kategorie nicht gefunden");

            _preise[kategorie] = preis;
            return Task.CompletedTask; // kein await nötig
        }
    }

}
