using MongoDB.Bson;
using MongoDB.Driver;
using Poll.N.Quiz.NuGet.IntegrationTests.EventStore.WriteOnly;
using Poll.N.Quiz.Settings.Domain.Internal;
using Poll.N.Quiz.Settings.Domain.ValueObjects;

namespace Poll.N.Quiz.NuGet.IntegrationTests.EventStore.ReadOnly;

public class SeededMongoDbFixture : MongoDbFixture
{
    private const string DatabaseName = "mongodb";
    private const string CollectionName = "settingsEvents";

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await CreateCollectionAndIndexesAsync();
        await SeedCollectionWithEventsAsync();
    }

    private async Task CreateCollectionAndIndexesAsync()
    {
        if (MongoClient is null)
            throw new InvalidOperationException("MongoDb container is not started or healthy");

        var compoundIndexModel = new CreateIndexModel<BsonDocument>(
            Builders<BsonDocument>.IndexKeys
                .Ascending(nameof(SettingsEvent.TimeStamp))
                .Text(nameof(SettingsEvent.ServiceName))
                .Text(nameof(SettingsEvent.EnvironmentName))
                .Text(nameof(SettingsEvent.EventType)));

        var _settingsEventCollection = MongoClient
            .GetDatabase(DatabaseName)
            .GetCollection<BsonDocument>(CollectionName);

        var createdIndex =
            await _settingsEventCollection.Indexes.CreateOneAsync(compoundIndexModel);
        if (createdIndex is null)
            throw new InvalidOperationException("Failed to create index for settingsUpdateEvents collection");

    }

    private async Task SeedCollectionWithEventsAsync()
    {
        if (MongoClient is null)
            return;

        var settingsEventCollection = MongoClient
            .GetDatabase(DatabaseName).GetCollection<BsonDocument>(CollectionName);

        var settingsEvents =
            TestSettingsEventFactory.CreateSettingsEvents().Select(se => se.ToBsonDocument());

        await settingsEventCollection.InsertManyAsync(settingsEvents);
    }
}
