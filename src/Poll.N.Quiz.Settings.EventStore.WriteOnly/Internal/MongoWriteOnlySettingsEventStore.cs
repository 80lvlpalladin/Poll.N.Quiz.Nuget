using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Poll.N.Quiz.Settings.Domain.ValueObjects;

namespace Poll.N.Quiz.Settings.EventStore.WriteOnly.Internal;

internal class MongoWriteOnlySettingsEventStore : IWriteOnlySettingsEventStore, IDisposable
{
    private readonly IMongoCollection<BsonDocument> _settingsEventCollection;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    internal const string DatabaseName = "mongodb";
    internal const string CollectionName = "settingsEvents";

    public MongoWriteOnlySettingsEventStore(IMongoClient mongoClient)
    {
        _settingsEventCollection = mongoClient
            .GetDatabase(DatabaseName)
            .GetCollection<BsonDocument>(CollectionName);

        InitializeIndexesForSettingsEventCollection();
    }

    /// <returns>If save was successful</returns>
    public async Task<bool> SaveAsync
        (SettingsEvent @event, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            if (!await IsValidAsync(@event))
                return false;

            var bsonDocument = @event.ToBsonDocument();
            var insertOptions = new InsertOneOptions();
            await _settingsEventCollection.InsertOneAsync
                (bsonDocument, insertOptions, cancellationToken);
            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<bool> IsValidAsync(SettingsEvent @event)
    {
        if (@event.EventType is SettingsEventType.CreateEvent)
        {
            var anyEventsExist = await _settingsEventCollection
                .AsQueryable()
                .AnyAsync(se =>
                    se[nameof(SettingsMetadata.ServiceName)].AsString == @event.Metadata.ServiceName &&
                    se[nameof(SettingsMetadata.EnvironmentName)].AsString == @event.Metadata.EnvironmentName);

            return !anyEventsExist;
        }

        if (@event.EventType is SettingsEventType.UpdateEvent)
        {
            var lastSavedEvent =
                _settingsEventCollection.AsQueryable().Last(se =>
                    se[nameof(SettingsMetadata.ServiceName)].AsString == @event.Metadata.ServiceName &&
                    se[nameof(SettingsMetadata.EnvironmentName)].AsString == @event.Metadata.EnvironmentName);

            if (lastSavedEvent is null)
                return false;

            if(@event.TimeStamp <= lastSavedEvent[1][nameof(@event.TimeStamp)].AsInt32)
                return false;
        }

        return true;
    }

    private void InitializeIndexesForSettingsEventCollection()
    {
        var compoundIndexModel = new CreateIndexModel<BsonDocument>(
            Builders<BsonDocument>.IndexKeys
                .Ascending(nameof(SettingsEvent.TimeStamp))
                .Text(nameof(SettingsMetadata.ServiceName))
                .Text(nameof(SettingsMetadata.EnvironmentName))
                .Text(nameof(SettingsEvent.EventType)));

        var createdIndex = _settingsEventCollection.Indexes.CreateOne(compoundIndexModel);

        if (createdIndex is null)
            throw new InvalidOperationException("Failed to create index for SettingsEvent collection");

    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _semaphore.Dispose();
    }
}
