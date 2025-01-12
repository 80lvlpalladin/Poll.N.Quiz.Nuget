using System.Text.Json;
using Poll.N.Quiz.Settings.Projection.ReadOnly.Entities;

namespace Poll.N.Quiz.Settings.Projection.ReadOnly;

public interface IReadOnlySettingsProjection
{
    public Task<(string settingsJson, uint lastUpdatedTimestamp)?> GetAsync
        (string serviceName, string environmentName);

    public Task<IReadOnlyCollection<SettingsMetadata>> GetAllSettingsMetadataAsync
        (CancellationToken cancellationToken = default);

    public Task<bool> IsEmptyAsync();
}
