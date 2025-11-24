using MongoDB.Driver;
using KinoAppCore.Documents;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KinoAppCore.Services
{
    public class StatsService
    {
        private readonly IMongoCollection<DailyShowRevenue> _statsCollection;

        public StatsService(IMongoClient mongoClient)
        {
            var db = mongoClient.GetDatabase("stats");
            _statsCollection = db.GetCollection<DailyShowRevenue>("daily_revenue");
        }

        // READ: Alle Statistiken holen
        public async Task<List<DailyShowRevenue>> GetAllAsync()
        {
            return await _statsCollection.Find(_ => true).ToListAsync();
        }

        // CREATE: Manuell Statistik anlegen (falls nötig für Tests)
        public async Task CreateAsync(DailyShowRevenue stat)
        {
            await _statsCollection.InsertOneAsync(stat);
        }
    }
}