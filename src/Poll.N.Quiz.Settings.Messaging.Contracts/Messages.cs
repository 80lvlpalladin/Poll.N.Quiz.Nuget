namespace Poll.N.Quiz.Settings.Messaging.Contracts;

public sealed record SettingsEvent(
    SettingsEventType EventType,
    uint TimeStamp,
    string ServiceName,
    string EnvironmentName,
    string JsonData);

public enum SettingsEventType
{
    CreateEvent,
    UpdateEvent
}


/*public record SettingsUpdateEvent(
    uint TimeStamp,
    string ServiceName,
    string EnvironmentName,
    Operation SettingsPatch)
    : SettingsEvent(TimeStamp, ServiceName, EnvironmentName);

public record SettingsCreateEvent(
    uint TimeStamp,
    string ServiceName,
    string EnvironmentName,
    JsonDocument Settings)
    : SettingsEvent(TimeStamp, ServiceName, EnvironmentName);

public abstract record SettingsEvent(
    uint TimeStamp,
    string ServiceName,
    string EnvironmentName)
{
    public string EventType => GetType().Name;
    public uint TimeStamp { get; init; } = TimeStamp;
    public string ServiceName { get; init; } = ServiceName;
    public string EnvironmentName { get; init; } = EnvironmentName;
}*/
