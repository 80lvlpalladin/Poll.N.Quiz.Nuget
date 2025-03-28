using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Poll.N.Quiz.Settings.Domain.Extensions;
using Poll.N.Quiz.Settings.Domain.ValueObjects;

namespace Poll.N.Quiz.Settings.EventStore.ReadOnly.Internal;

internal class MongoReadOnlySettingsEventStore(IMongoClient mongoClient)
    : IReadOnlySettingsEventStore
{
    private readonly IMongoCollection<BsonDocument> _settingsUpdateEventCollection = mongoClient
        .GetDatabase("mongodb")
        .GetCollection<BsonDocument>("settingsEvents");

    public async Task<SettingsEvent[]> GetAsync
        (SettingsMetadata settingsMetadata, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(settingsMetadata.ServiceName) ||
            string.IsNullOrWhiteSpace(settingsMetadata.EnvironmentName))
            return [];

        var bsonDocuments = await _settingsUpdateEventCollection
            .AsQueryable()
            .Where(bson =>
                bson[nameof(SettingsMetadata.ServiceName)].AsString == settingsMetadata.ServiceName &&
                bson[nameof(SettingsMetadata.EnvironmentName)].AsString == settingsMetadata.EnvironmentName)
            .ToListAsync(cancellationToken);

        return bsonDocuments.Select(bsonDoc => bsonDoc.ToSettingsEvent()).ToArray();
    }


    public async Task<SettingsEvent[]> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var bsonDocuments = await _settingsUpdateEventCollection
            .AsQueryable()
            .ToListAsync(cancellationToken);

        return bsonDocuments.Select(bsonDoc => bsonDoc.ToSettingsEvent()).ToArray();
    }
}
