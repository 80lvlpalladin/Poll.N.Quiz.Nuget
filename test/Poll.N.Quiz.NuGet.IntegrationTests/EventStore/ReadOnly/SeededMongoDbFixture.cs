using MongoDB.Bson;
using Poll.N.Quiz.NuGet.IntegrationTests.EventStore.WriteOnly;
using Poll.N.Quiz.Settings.Domain;
using Poll.N.Quiz.Settings.EventStore.WriteOnly.Internal;

namespace Poll.N.Quiz.NuGet.IntegrationTests.EventStore.ReadOnly;

public class SeededMongoDbFixture : MongoDbFixture
{
    private const string DatabaseName = "mongodb";
    private const string CollectionName = "settingsEvents";

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        CreateCollectionAndIndexes();
        await SeedCollectionWithEventsAsync();
    }

    private void CreateCollectionAndIndexes()
    {
        if (MongoClient is null)
            throw new InvalidOperationException("MongoDb container is not started or healthy");

        var settingsEventCollection = MongoClient
            .GetDatabase(DatabaseName)
            .GetCollection<BsonDocument>(CollectionName);

        MongoWriteOnlySettingsEventStore.InitializeIndexesForSettingsEventCollection(settingsEventCollection);
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
