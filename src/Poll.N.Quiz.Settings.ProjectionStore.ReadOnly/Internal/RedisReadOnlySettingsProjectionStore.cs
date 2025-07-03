using Poll.N.Quiz.Settings.Domain.ValueObjects;

namespace Poll.N.Quiz.Settings.ProjectionStore.ReadOnly.Internal;

internal class RedisReadOnlySettingsProjectionStore(IReadOnlyKeyValueStorage redisStorage)
    : IReadOnlySettingsProjectionStore
{
    private static string CreateRedisKey(SettingsMetadata settingsMetadata) =>
        $"{settingsMetadata.ServiceName.ToLowerInvariant()}__{settingsMetadata.EnvironmentName.ToLowerInvariant()}";

    private static SettingsMetadata DeconstructRedisKey(string key)
    {
        var keySegments = key.Split("__");
        return new SettingsMetadata(keySegments[0], keySegments[1]);
    }

    public Task<SettingsProjection?> GetAsync(SettingsMetadata settingsMetadata)
    {
        var redisKey = CreateRedisKey(settingsMetadata);

        return redisStorage.GetAsync<SettingsProjection>(redisKey);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<IReadOnlyCollection<SettingsMetadata>> GetSettingsMetadataAsync
        (string? serviceName = null, CancellationToken cancellationToken = default)
    {
        var keyPrefix = serviceName is null ? "*" : $"{serviceName}__*";

        var keys = await redisStorage.ListKeysAsync(keyPrefix, cancellationToken);

        return keys
            .Select(DeconstructRedisKey)
            .ToArray();
    }

    public Task<bool> IsEmptyAsync() => redisStorage.IsEmptyAsync();
}
