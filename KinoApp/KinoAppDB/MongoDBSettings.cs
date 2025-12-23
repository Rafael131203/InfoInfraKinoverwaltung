namespace KinoAppDB
{
    /// <summary>
    /// Configuration model for MongoDB integration.
    /// </summary>
    /// <remarks>
    /// Bound from application configuration and used to initialize MongoDB clients
    /// for statistics and projection storage.
    /// </remarks>
    public class MongoDBSettings
    {
        /// <summary>
        /// MongoDB connection string.
        /// </summary>
        public string ConnectionString { get; set; } = null!;

        /// <summary>
        /// Name of the MongoDB database to use.
        /// </summary>
        public string DatabaseName { get; set; } = null!;
    }
}
