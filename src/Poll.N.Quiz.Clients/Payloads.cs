// ReSharper disable once CheckNamespace
namespace Poll.N.Quiz.Clients.Payloads;

#region Settings API Payloads

public sealed record GetAllSettingsMetadataResponse(IEnumerable<SettingsMetadataResponse> Metadata);
public sealed record SettingsMetadataResponse(string ServiceName, IEnumerable<string> EnvironmentNames);

public sealed record GetSettingsContentResponse(
    string JsonData,
    uint LastUpdatedTimestamp,
    uint Version);

public sealed record CreateSettingsRequest(
    uint TimeStamp,
    uint Version,
    string ServiceName,
    string EnvironmentName,
    string SettingsJson);

public sealed record UpdateSettingsRequest(
    uint TimeStamp,
    uint Version,
    string ServiceName,
    string EnvironmentName,
    string SettingsPatchJson);

public record ReloadProjectionRequest(string ServiceName, string EnvironmentName);

#endregion
