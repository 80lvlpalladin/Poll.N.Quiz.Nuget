using Poll.N.Quiz.Settings.Domain.ValueObjects;

namespace Poll.N.Quiz.Settings.FileStore.ReadOnly;

public interface IReadOnlySettingsFileStore
{
    public Task<string> GetSettingsContentAsync(
        SettingsMetadata settingsMetadata,
        CancellationToken cancellationToken = default);

    public IEnumerable<SettingsMetadata> GetAllSettingsMetadata
        (CancellationToken cancellationToken = default);
}
