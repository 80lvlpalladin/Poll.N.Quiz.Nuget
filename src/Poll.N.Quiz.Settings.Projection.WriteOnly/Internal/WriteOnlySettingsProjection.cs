using Microsoft.Extensions.Options;

namespace Poll.N.Quiz.Settings.Projection.WriteOnly.Internal;

internal class WriteOnlySettingsProjection
    (IWriteOnlyKeyValueStorage storage, IOptions<SettingsProjectionOptions> options)
    : IWriteOnlySettingsProjection
{
    private readonly TimeSpan _expiryTime = TimeSpan.FromHours(options.Value.ExpirationTimeHours);

    private static string CreateRedisKey(string serviceName, string environmentName) =>
        $"{serviceName}__{environmentName}";

    public Task SaveProjectionAsync(
        uint timeStamp,
        string serviceName,
        string environmentName,
        string settingsJson,
        CancellationToken cancellationToken = default)
    {
        var redisKey = CreateRedisKey(serviceName, environmentName);

        var projectionModel = new ProjectionModel(timeStamp, settingsJson);

        return storage.SetAsync(redisKey, projectionModel, _expiryTime);
    }
}

internal record ProjectionModel(uint LastUpdatedTimeStamp, string SettingsJson);



