using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace KinoAppCore.Services
{
    public class StatsService
    {
        private readonly IMongoDatabase _db;

        public StatsService(IMongoDatabase db)
        {
            _db = db;
        }

        public async Task<List<BsonDocument>> GetAllAsync(string collectionName)
        {
            var collection = _db.GetCollection<BsonDocument>(collectionName);
            return await collection.Find(FilterDefinition<BsonDocument>.Empty).ToListAsync();
        }

        public async Task AddAsync(string collectionName, BsonDocument doc)
        {
            var collection = _db.GetCollection<BsonDocument>(collectionName);
            await collection.InsertOneAsync(doc);
        }
    }

}
