using Poll.N.Quiz.Settings.Domain.ValueObjects;

namespace Poll.N.Quiz.Settings.ProjectionStore.ReadOnly;

public interface IReadOnlySettingsProjectionStore
{
    public Task<SettingsProjection?> GetAsync(SettingsMetadata settingsMetadata);

    public Task<IReadOnlyCollection<SettingsMetadata>> GetAllSettingsMetadataAsync
        (CancellationToken cancellationToken = default);

    public Task<bool> IsEmptyAsync();
}
