using MongoDB.Bson;
using Poll.N.Quiz.Settings.Domain.ValueObjects;

namespace Poll.N.Quiz.Settings.Domain.Extensions;

public static class BsonDocumentExtensions
{
    public static SettingsEvent ToSettingsEvent(this BsonDocument bsonDocument)
    {
        var eventType = (SettingsEventType) bsonDocument[nameof(SettingsEvent.EventType)].AsInt32;
        var timeStamp = (uint) bsonDocument[nameof(SettingsEvent.TimeStamp)].AsInt32;
        var version = (uint) bsonDocument[nameof(SettingsEvent.Version)].AsInt32;
        var serviceName = bsonDocument[nameof(SettingsMetadata.ServiceName)].AsString;
        var environmentName = bsonDocument[nameof(SettingsMetadata.EnvironmentName)].AsString;
        var jsonData = bsonDocument[nameof(SettingsEvent.JsonData)].AsString;

        return new SettingsEvent(
            eventType,
            new SettingsMetadata(serviceName, environmentName),
            timeStamp,
            version,
            jsonData);
    }
}
