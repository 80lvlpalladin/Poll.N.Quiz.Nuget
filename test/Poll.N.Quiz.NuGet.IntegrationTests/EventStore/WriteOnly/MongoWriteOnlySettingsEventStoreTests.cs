using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Poll.N.Quiz.Settings.Domain;
using Poll.N.Quiz.Settings.EventStore.WriteOnly.Internal;
using Poll.N.Quiz.Settings.Domain.Extensions;
using Poll.N.Quiz.Settings.Domain.ValueObjects;

namespace Poll.N.Quiz.NuGet.IntegrationTests.EventStore.WriteOnly;

public class MongoWriteOnlySettingsEventStoreTests()
{
    private static readonly MongoDbFixture _mongoDbFixture = new();

    [Before(Class)]
    public static Task InitializeAsync() => _mongoDbFixture.InitializeAsync();

    [After(Class)]
    public static ValueTask DisposeAsync() => _mongoDbFixture.DisposeAsync();

    [After(Test)]
    public async Task ClearMongoCollectionAsync()
    {
        var collection = GetSettingsUpdateMongoCollection();
        var filter = Builders<BsonDocument>.Filter.Empty;
        var result = await collection.DeleteManyAsync(filter);
    }

    [Test]
    [NotInParallel]
    public async Task EventStore_CreatesIndexes_WhenInitialized()
    {
        // Arrange
        const string expectedIndexName =
            "TimeStamp_1_ServiceName_text_EnvironmentName_text_EventType_text";

        // Act
        _ = new MongoWriteOnlySettingsEventStore(_mongoDbFixture.MongoClient!);

        // Assert
        var collection = GetSettingsUpdateMongoCollection();
        var actualIndexList = await (await collection.Indexes.ListAsync()).ToListAsync();

        await Assert.That(actualIndexList).IsNotNull();
        await Assert.That(actualIndexList).IsNotEmpty();
        await Assert.That(actualIndexList.Count).IsEqualTo(2);
        await Assert.That(actualIndexList.Select(i => i["name"].AsString))
            .Contains(expectedIndexName);
    }

    [Test]
    [NotInParallel]
    public async Task SaveAsync_WhenSavingSettingsCreateEvent_ReturnsTrueAndStoresEventInMongo_IfMongoIsEmpty()
    {
        // Arrange
        var eventStore = new MongoWriteOnlySettingsEventStore(_mongoDbFixture.MongoClient!);
        var eventToSave = TestSettingsEventFactory
            .CreateSettingsEvents()
            .First(se => se.EventType is SettingsEventType.CreateEvent);

        // Act
        var saveResult = await eventStore.SaveAsync(eventToSave);

        // Assert
        await Assert.That(saveResult).IsTrue();

        var savedBsonDocument = await GetSettingsUpdateMongoCollection()
            .AsQueryable()
            .Where(e => e[nameof(eventToSave.TimeStamp)] == eventToSave.TimeStamp)
            .FirstOrDefaultAsync();

        await Assert.That(savedBsonDocument).IsNotNull();

        var savedSettingsCreateEvent = savedBsonDocument.ToSettingsEvent();

        await Assert.That(savedSettingsCreateEvent.EventType).IsEqualTo(eventToSave.EventType)
            .And.IsEqualTo(SettingsEventType.CreateEvent);
        await Assert.That(savedSettingsCreateEvent.TimeStamp).IsEqualTo(eventToSave.TimeStamp);
        await Assert.That(savedSettingsCreateEvent.Metadata).IsEqualTo(eventToSave.Metadata);
        await Assert.That(savedSettingsCreateEvent.JsonData).IsEqualTo(eventToSave.JsonData);
    }

    [Test]
    [NotInParallel]
    public async Task SaveAsync_WhenSavingSettingsCreateEvent_ReturnsFalse_IfAlreadyPresentInMongo()
    {
        // Arrange
        var eventStore = new MongoWriteOnlySettingsEventStore(_mongoDbFixture.MongoClient!);
        var collection = GetSettingsUpdateMongoCollection();
        var eventToSave = TestSettingsEventFactory
            .CreateSettingsEvents()
            .First(se => se.EventType is SettingsEventType.CreateEvent);
        await collection.InsertOneAsync(eventToSave.ToBsonDocument());

        // Act
        var saveResult = await eventStore.SaveAsync(eventToSave);

        // Assert
        await Assert.That(saveResult).IsFalse();
    }

    [Test]
    [NotInParallel]
    public async Task SaveAsync_WhenSavingSettingsUpdateEvent_ReturnsTrue_IfEventIsNewerThanLastSavedEvent()
    {
        // Arrange
        var collection = GetSettingsUpdateMongoCollection();
        var serviceMetadata = new SettingsMetadata("service1", "environment1");
        var allEvents = TestSettingsEventFactory
            .CreateSettingsEvents()
            .Where(se => se.Metadata == serviceMetadata)
            .ToArray();

        await collection.InsertManyAsync(
            allEvents[..^1].Select(se => se.ToBsonDocument()));
        var eventToSave = allEvents.Last();
        var eventStore = new MongoWriteOnlySettingsEventStore(_mongoDbFixture.MongoClient!);


        // Act
        var saveResult = await eventStore.SaveAsync(eventToSave);

        // Assert
        await Assert.That(saveResult).IsTrue();

        var savedBsonDocument = await collection
            .AsQueryable()
            .Where(e => e[nameof(eventToSave.TimeStamp)] == eventToSave.TimeStamp)
            .FirstOrDefaultAsync();

        await Assert.That(savedBsonDocument).IsNotNull();

        var savedSettingsUpdateEvent = savedBsonDocument.ToSettingsEvent();

        await Assert.That(savedSettingsUpdateEvent.EventType).IsEqualTo(eventToSave.EventType)
            .And.IsEqualTo(SettingsEventType.UpdateEvent);
        await Assert.That(savedSettingsUpdateEvent.TimeStamp).IsEqualTo(eventToSave.TimeStamp);
        await Assert.That(savedSettingsUpdateEvent.Metadata).IsEqualTo(eventToSave.Metadata);
        await Assert.That(savedSettingsUpdateEvent.JsonData).IsEqualTo(eventToSave.JsonData);
    }

    [Test]
    [NotInParallel]
    public async Task SaveAsync_WhenSavingSettingsUpdateEvent_ReturnsFalse_IfEventIsOlderThanLastSavedEvent()
    {
        // Arrange
        var collection = GetSettingsUpdateMongoCollection();
        var serviceMetadata = new SettingsMetadata("service1", "environment1");
        var allEvents = TestSettingsEventFactory
            .CreateSettingsEvents()
            .Where(se => se.Metadata == serviceMetadata)
            .ToArray();

        await collection.InsertManyAsync(
            allEvents[..^1].Select(se => se.ToBsonDocument()));
        var eventToSave = allEvents.Last() with
        {
            TimeStamp = 1
        };

        var eventStore = new MongoWriteOnlySettingsEventStore(_mongoDbFixture.MongoClient!);

        // Act
        var saveResult = await eventStore.SaveAsync(eventToSave);

        // Assert
        await Assert.That(saveResult).IsFalse();
    }

    private IMongoCollection<BsonDocument> GetSettingsUpdateMongoCollection() =>
        _mongoDbFixture.MongoClient!
            .GetDatabase(MongoWriteOnlySettingsEventStore.DatabaseName)
            .GetCollection<BsonDocument>(MongoWriteOnlySettingsEventStore.CollectionName);
}
