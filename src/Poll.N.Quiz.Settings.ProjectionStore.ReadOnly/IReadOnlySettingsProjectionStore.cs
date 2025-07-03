using Poll.N.Quiz.Settings.Domain.ValueObjects;

namespace Poll.N.Quiz.Settings.ProjectionStore.ReadOnly;

public interface IReadOnlySettingsProjectionStore
{
    public Task<SettingsProjection?> GetAsync(SettingsMetadata settingsMetadata);

    /// <summary>
    /// Retrieves settings metadata for a service / all services.
    /// </summary>
    public Task<IReadOnlyCollection<SettingsMetadata>> GetSettingsMetadataAsync
        (string? serviceName = null, CancellationToken cancellationToken = default);

    public Task<bool> IsEmptyAsync();
}
