using LibrasJa.Application.Interfaces;
using LibrasJa.Domain.Entities;
using MongoDB.Driver;

namespace LibrasJa.Infrastructure.Mongo;

public class MongoAuditLogRepository : IAuditLogRepository
{
    private readonly IMongoCollection<AuditLog> _collection;

    public MongoAuditLogRepository(IMongoClient client, string databaseName, string collectionName)
    {
        var db = client.GetDatabase(databaseName);
        _collection = db.GetCollection<AuditLog>(collectionName);
    }

    public async Task AddAsync(AuditLog log)
    {
        await _collection.InsertOneAsync(log);
    }

    public async Task<IEnumerable<AuditLog>> GetAllAsync()
    {
        return await _collection.Find(_ => true)
            .SortByDescending(l => l.Timestamp)
            .Limit(100)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entity)
    {
        return await _collection.Find(l => l.Entity == entity)
            .SortByDescending(l => l.Timestamp)
            .Limit(100)
            .ToListAsync();
    }
}
