using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using ErrorOr;
using Json.Patch;
using Poll.N.Quiz.Settings.Domain.ValueObjects;

namespace Poll.N.Quiz.Settings.Domain;


public sealed class SettingsAggregate
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public SettingsAggregate(SettingsMetadata metadata, SettingsProjection? currentProjection = null)
    {
        CurrentProjection = currentProjection;
        Metadata = metadata;
    }

    public SettingsProjection? CurrentProjection { get; private set; }
    public  SettingsMetadata Metadata { get; }

    public ErrorOr<Success> ApplyEvent(SettingsEvent settingsEvent)
    {
        var validateResult = Validate(settingsEvent);

        if (validateResult.IsError)
            return validateResult.FirstError;

        return CreateOrUpdateCurrentProjection(settingsEvent);
    }


    private ErrorOr<Success> CreateOrUpdateCurrentProjection(SettingsEvent settingsEvent)
    {
        if (settingsEvent.EventType is SettingsEventType.CreateEvent && CurrentProjection is null)
        {
            CurrentProjection = new SettingsProjection(
                settingsEvent.JsonData,
                settingsEvent.TimeStamp,
                settingsEvent.Version);

            return Result.Success;
        }

        if (settingsEvent.EventType is SettingsEventType.UpdateEvent && CurrentProjection is not null)
        {
            var applyJsonPatchResult =
                ApplyJsonPatch(CurrentProjection.JsonData, settingsEvent.JsonData);

            if (applyJsonPatchResult.IsError)
                return applyJsonPatchResult.FirstError;

            CurrentProjection = new SettingsProjection(
                applyJsonPatchResult.Value,
                settingsEvent.TimeStamp,
                settingsEvent.Version);

            return Result.Success;
        }

        return Error.Failure("SettingsAggregate state is invalid");
    }

    private ErrorOr<Success> Validate(SettingsEvent settingsEvent)
    {
        if(settingsEvent.Metadata != Metadata)
            return Error.Validation("SettingsEvent metadata does not match SettingsAggregate metadata.");

        if (settingsEvent.EventType is SettingsEventType.CreateEvent)
        {
            if(settingsEvent.Version is not 0)
                return Error.Validation("SettingsCreateEvent version must be 0.");

            if(CurrentProjection is not null)
                return Error.Validation("Projection already exists for this event.");

        }
        else if(settingsEvent.EventType is SettingsEventType.UpdateEvent)
        {
            if(CurrentProjection is null)
                return Error.Validation("Projection does not exist for this event.");

            if (settingsEvent.Version != CurrentProjection.Version + 1)
                return Error.Validation("Event version must be one greater than the current version.");

            if (settingsEvent.TimeStamp < CurrentProjection.LastUpdatedTimestamp)
                return Error.Validation("Event timestamp must be greater than the current timestamp.");
        }
        else
        {
            return Error.Validation("Unsupported event type");
        }

        return Result.Success;
    }

    internal static ErrorOr<string> ApplyJsonPatch(string originalJson, string jsonPatch)
    {
        var eventJsonPatch = JsonSerializer.Deserialize<JsonPatch>(jsonPatch);

        if (eventJsonPatch is null)
            return Error.Validation("Failed to deserialize event's json patch.");

        var currentProjectionJsonNode = JsonNode.Parse(originalJson);

        if (currentProjectionJsonNode is null)
            return Error.Validation("Failed to parse current projection's json data.");

        var patchResult = eventJsonPatch.Apply(currentProjectionJsonNode);

        if (!patchResult.IsSuccess || patchResult.Result is null)
            return Error.Failure("Failed to apply patch to current projection.");

        return patchResult.Result.ToJsonString();
    }
}
