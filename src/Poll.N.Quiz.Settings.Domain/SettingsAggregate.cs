using System.Text.Json;
using System.Text.Json.Nodes;
using ErrorOr;
using Json.Patch;
using Poll.N.Quiz.Settings.Domain.ValueObjects;

namespace Poll.N.Quiz.Settings.Domain;

//TODO cover with tests
public sealed class SettingsAggregate
{
    public SettingsAggregate(SettingsEvent settingsCreateEvent)
    {
        if (settingsCreateEvent.EventType is not SettingsEventType.CreateEvent ||
            settingsCreateEvent.Version is not 0)
            throw new ArgumentException("Provided settings event is not of type SettingsCreateEvent");

        CurrentProjection = new SettingsProjection(
            settingsCreateEvent.JsonData,
            settingsCreateEvent.TimeStamp,
            settingsCreateEvent.Version);

        Metadata = settingsCreateEvent.Metadata;
    }

    public SettingsAggregate(SettingsMetadata metadata, SettingsProjection projection)
    {
        Metadata = metadata;
        CurrentProjection = projection;
    }

    public SettingsProjection CurrentProjection { get; private set; }

    public SettingsMetadata Metadata { get; }


    public ErrorOr<Success> ApplyEvent(SettingsEvent settingsUpdateEvent)
    {
        if (settingsUpdateEvent.EventType is not SettingsEventType.UpdateEvent)
            return Error.Validation("Provided event is not of type SettingsUpdateEvent");

        if (settingsUpdateEvent.Version != CurrentProjection.Version + 1)
            return Error.Validation("Event version must be one greater than the current version.");

        if (settingsUpdateEvent.TimeStamp < CurrentProjection.LastUpdatedTimestamp)
            return Error.Validation("Event timestamp must be greater than the current timestamp.");

        if (settingsUpdateEvent.Metadata.ServiceName != Metadata.ServiceName)
            return Error.Validation("Service name must match the current service name.");

        if (settingsUpdateEvent.Metadata.EnvironmentName != Metadata.EnvironmentName)
            return Error.Validation("Environment name must match the current environment name.");

        return UpdateCurrentProjection(settingsUpdateEvent);
    }

    private ErrorOr<Success> UpdateCurrentProjection(SettingsEvent settingsUpdateEvent)
    {
        var eventJsonPatch = JsonSerializer.Deserialize<JsonPatch>(settingsUpdateEvent.JsonData);

        if(eventJsonPatch is null)
            return Error.Validation("Failed to deserialize event's json patch.");

        var currentProjectionJsonNode = JsonNode.Parse(CurrentProjection.JsonData);

        if(currentProjectionJsonNode is null)
            return Error.Validation("Failed to parse current projection's json data.");

        var patchResult = eventJsonPatch.Apply(currentProjectionJsonNode);

        if(!patchResult.IsSuccess || patchResult.Result is null)
            return Error.Failure("Failed to apply patch to current projection.");

        var newProjectionJson = patchResult.Result.ToJsonString();

        CurrentProjection = new SettingsProjection(
            newProjectionJson,
            settingsUpdateEvent.TimeStamp,
            settingsUpdateEvent.Version);

        return Result.Success;
    }
}
