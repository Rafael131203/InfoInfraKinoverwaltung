using KinoAppCore.Abstractions;
using KinoAppShared.Messaging;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppCore.Services
{
    // Im Service oder Controller
    // Implementiere das Interface
    public class TicketService
    {
        private readonly IMongoCollection<TicketStat> _stats;

        // Dependency Injection der IMongoDatabase
        public TicketService(IMongoDatabase db)
        {
            _stats = db.GetCollection<TicketStat>("stats");
        }

        // READ (Alle)
        public async Task<List<TicketStat>> GetAllAsync()
        {
            return await _stats.Find(_ => true).ToListAsync();
        }

        // CREATE
        public async Task CreateAsync(TicketStat stat)
        {
            await _stats.InsertOneAsync(stat);
        }
    }

}
