using Poll.N.Quiz.Settings.Domain.ValueObjects;

namespace Poll.N.Quiz.Settings.Projection.ReadOnly.Internal;

internal class RedisReadOnlySettingsProjection(IReadOnlyKeyValueStorage redisStorage)
    : IReadOnlySettingsProjection
{
    private static string CreateRedisKey(SettingsMetadata settingsMetadata) =>
        $"{settingsMetadata.ServiceName}__{settingsMetadata.EnvironmentName}";

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

    public async Task<IReadOnlyCollection<SettingsMetadata>> GetAllSettingsMetadataAsync
        (CancellationToken cancellationToken = default)
    {
        var allKeys = await redisStorage.ListAllKeysAsync(cancellationToken);

        return allKeys
            .Select(DeconstructRedisKey)
            .ToArray();
    }

    public Task<bool> IsEmptyAsync() => redisStorage.IsEmptyAsync();
}
