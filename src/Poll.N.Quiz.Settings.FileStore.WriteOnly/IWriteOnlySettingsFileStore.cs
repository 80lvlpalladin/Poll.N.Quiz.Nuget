using Poll.N.Quiz.Settings.Domain.ValueObjects;

namespace Poll.N.Quiz.Settings.FileStore.WriteOnly;

public interface IWriteOnlySettingsFileStore
{
    public Task SaveAsync(
        SettingsMetadata settingsMetadata,
        string jsonData,
        CancellationToken cancellationToken = default);
}
