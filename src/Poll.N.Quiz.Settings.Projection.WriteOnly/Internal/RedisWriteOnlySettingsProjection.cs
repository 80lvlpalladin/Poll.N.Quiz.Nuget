using Microsoft.Extensions.Options;
using Poll.N.Quiz.Settings.Domain.ValueObjects;

namespace Poll.N.Quiz.Settings.Projection.WriteOnly.Internal;

internal class RedisWriteOnlySettingsProjection
    (IWriteOnlyKeyValueStorage storage, IOptions<SettingsProjectionOptions> options)
    : IWriteOnlySettingsProjection
{
    private readonly TimeSpan _expiryTime = TimeSpan.FromHours(options.Value.ExpirationTimeHours);

    private static string CreateRedisKey(SettingsMetadata settingsMetadata) =>
        $"{settingsMetadata.ServiceName}__{settingsMetadata.EnvironmentName}";

    public Task SaveProjectionAsync(
        SettingsProjection projection,
        SettingsMetadata settingsMetadata,
        CancellationToken cancellationToken = default)
    {
        var redisKey = CreateRedisKey(settingsMetadata);
        return storage.SetAsync(redisKey, projection, _expiryTime);
    }
}
