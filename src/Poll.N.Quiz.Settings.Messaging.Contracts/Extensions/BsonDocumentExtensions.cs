using MongoDB.Bson;

namespace Poll.N.Quiz.Settings.Messaging.Contracts.Extensions;

public static class BsonDocumentExtensions
{
    public static SettingsEvent ToSettingsEvent(this BsonDocument bsonDocument)
    {
        var eventType = (SettingsEventType) bsonDocument[nameof(SettingsEvent.EventType)].AsInt32;
        var timeStamp = (uint) bsonDocument[nameof(SettingsEvent.TimeStamp)].AsInt32;
        var serviceName = bsonDocument[nameof(SettingsEvent.ServiceName)].AsString;
        var environmentName = bsonDocument[nameof(SettingsEvent.EnvironmentName)].AsString;
        var jsonData = bsonDocument[nameof(SettingsEvent.JsonData)].AsString;

        return new SettingsEvent(eventType, timeStamp, serviceName, environmentName, jsonData);
    }
}
