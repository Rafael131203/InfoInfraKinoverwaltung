using MongoDB.Driver;
using KinoAppCore.Documents;

namespace KinoAppCore.Services
{
    /// <summary>
    /// Provides read/write access to revenue statistics stored in MongoDB.
    /// </summary>
    /// <remarks>
    /// This service targets the <c>stats</c> database and the <c>daily_revenue</c> collection, which stores
    /// <see cref="DailyShowRevenue"/> documents for reporting and analytics.
    /// </remarks>
    public class StatsService
    {
        private readonly IMongoCollection<DailyShowRevenue> _statsCollection;

        /// <summary>
        /// Creates a new <see cref="StatsService"/>.
        /// </summary>
        /// <param name="mongoClient">MongoDB client used to access the statistics database.</param>
        public StatsService(IMongoClient mongoClient)
        {
            var db = mongoClient.GetDatabase("stats");
            _statsCollection = db.GetCollection<DailyShowRevenue>("daily_revenue");
        }

        /// <summary>
        /// Returns all stored revenue statistic documents.
        /// </summary>
        /// <returns>A list of <see cref="DailyShowRevenue"/> documents (may be empty).</returns>
        public async Task<List<DailyShowRevenue>> GetAllAsync()
        {
            return await _statsCollection.Find(_ => true).ToListAsync();
        }

        /// <summary>
        /// Inserts a revenue statistic document.
        /// </summary>
        /// <param name="stat">The statistic document to insert.</param>
        public async Task CreateAsync(DailyShowRevenue stat)
        {
            await _statsCollection.InsertOneAsync(stat);
        }
    }
}
