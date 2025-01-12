using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Poll.N.Quiz.Settings.Messaging.Contracts;
using Poll.N.Quiz.Settings.Messaging.Contracts.Extensions;

namespace Poll.N.Quiz.Settings.EventStore.ReadOnly.Internal;

internal class MongoReadOnlySettingsEventStore(IMongoClient mongoClient) : IReadOnlySettingsEventStore
{
    private readonly IMongoCollection<BsonDocument> _settingsUpdateEventCollection = mongoClient
        .GetDatabase("mongodb")
        .GetCollection<BsonDocument>("settingsEvents");

    public async Task<SettingsEvent[]> GetEventsAsync
        (string serviceName, string environmentName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(serviceName) || string.IsNullOrWhiteSpace(environmentName))
            return [];

        var bsonDocuments = await _settingsUpdateEventCollection
            .AsQueryable()
            .Where(bson =>
                bson["ServiceName"].AsString == serviceName &&
                bson["EnvironmentName"].AsString == environmentName)
            .ToListAsync(cancellationToken);

        return bsonDocuments.Select(bsonDoc => bsonDoc.ToSettingsEvent()).ToArray();
    }


    public async Task<SettingsEvent[]> GetAllEventsAsync(CancellationToken cancellationToken = default)
    {
        var bsonDocuments = await _settingsUpdateEventCollection
            .AsQueryable()
            .ToListAsync(cancellationToken);

        return bsonDocuments.Select(bsonDoc => bsonDoc.ToSettingsEvent()).ToArray();
    }


    //private static SettingsEvent CreateSettingsEventFrom(BsonDocument bsonDocument)
    //{
    //    return BsonSerializer.Deserialize<IDictionary<string, object>>(bsonDocument);
        /*var eventType = bsonDocument[nameof(SettingsEvent.EventType)].AsString;

        switch (eventType)
        {
            case nameof(SettingsUpdateEvent):
            {
                var settingsPatchJson = bsonDocument[nameof(SettingsUpdateEvent.SettingsPatch)].ToJson()
                                        ?? throw new InvalidOperationException(); //TODO add message

                var settingsPatch = JsonConvert.DeserializeObject<Operation>(settingsPatchJson)
                                    ?? throw new InvalidOperationException(); //TODO add message

                return new SettingsUpdateEvent(
                    (uint)bsonDocument["TimeStamp"].AsInt64,
                    bsonDocument["ServiceName"].AsString,
                    bsonDocument["EnvironmentName"].AsString,
                    settingsPatch);
            }
            case nameof(SettingsCreateEvent):
            {
                var settings =
                    JsonSerializer.Deserialize<JsonDocument>(bsonDocument[nameof(SettingsCreateEvent.Settings)].ToJson())
                    ?? throw new InvalidOperationException(); //TODO add message

                return new SettingsCreateEvent(
                    (uint)bsonDocument["TimeStamp"].AsInt64,
                    bsonDocument["ServiceName"].AsString,
                    bsonDocument["EnvironmentName"].AsString,
                    settings);
            }
            default:
                throw new InvalidOperationException($"Unknown event type: {eventType}");
        }*/
    //}
}
