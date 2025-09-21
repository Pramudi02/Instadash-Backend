using Backend.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

public class MongoService
{
    private readonly IMongoDatabase _database;

    public MongoService(IOptions<Backend.Models.MongoDBSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<Campaign> Campaigns => _database.GetCollection<Campaign>("campaigns");
}

public class MongoDBSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}