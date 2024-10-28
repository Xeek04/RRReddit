using MongoDB.Driver;

namespace RRReddit.Data
{
    public class MongoDatabase
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoDatabase _database;
        public MongoDatabase(IConfiguration configuration)
        {
            _configuration = configuration;

            var connectionString = _configuration.GetConnectionString("DbConnection");
            var Url = MongoUrl.Create(connectionString);
            var mongoClient = new MongoClient(Url);
            _database = mongoClient.GetDatabase("RRReddit");
        }

        public IMongoDatabase? Database => _database;
    }
}
