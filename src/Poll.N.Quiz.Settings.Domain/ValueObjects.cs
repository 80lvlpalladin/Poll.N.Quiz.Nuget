// ReSharper disable once CheckNamespace
namespace Poll.N.Quiz.Settings.Domain.ValueObjects;

public sealed record SettingsMetadata(string ServiceName, string EnvironmentName);

public sealed record SettingsEvent(
    SettingsEventType EventType,
    SettingsMetadata Metadata,
    uint TimeStamp,
    uint Version,
    string JsonData)
{
    public uint TimeStamp { get; init; } = TimeStamp == 0 ?
        Convert.ToUInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds()) : TimeStamp;
}

public enum SettingsEventType
{
    CreateEvent,
    UpdateEvent
}

public sealed record SettingsProjection(
    string JsonData,
    uint LastUpdatedTimestamp,
    uint Version);
