using System.Text.Json;
using Poll.N.Quiz.Settings.Projection.ReadOnly.Entities;

namespace Poll.N.Quiz.Settings.Projection.ReadOnly.Internal;

internal class ReadOnlySettingsProjection(IReadOnlyKeyValueStorage redisStorage)
    : IReadOnlySettingsProjection
{
    private static string CreateRedisKey(string serviceName, string environmentName) =>
        $"{serviceName}__{environmentName}";

    private static (string serviceName, string environmentName) DeconstructRedisKey(string key)
    {
        var keySegments = key.Split("__");
        return new ValueTuple<string, string>(keySegments[0], keySegments[1]);
    }

    public async Task<(string settingsJson, uint lastUpdatedTimestamp)?>
        GetAsync(string serviceName, string environmentName)
    {
        var redisKey = CreateRedisKey(serviceName, environmentName);
        var projectionModel = await redisStorage.GetAsync<ProjectionModel>(redisKey);

        if (projectionModel is null)
            return null;

        return (projectionModel.SettingsJson, projectionModel.LastUpdatedTimeStamp);
    }


    public async Task<IReadOnlyCollection<SettingsMetadata>> GetAllSettingsMetadataAsync
        (CancellationToken cancellationToken = default)
    {
        var allKeys = await redisStorage.ListAllKeysAsync(cancellationToken);

        return allKeys
            .Select(DeconstructRedisKey)
            .GroupBy(se => se.serviceName)
            .Select(grouping => new SettingsMetadata(
                grouping.Key,
                grouping.Select(se => se.environmentName).ToArray()))
            .ToArray();
    }

    public Task<bool> IsEmptyAsync() => redisStorage.IsEmptyAsync();


}

internal record ProjectionModel(uint LastUpdatedTimeStamp, string SettingsJson);

