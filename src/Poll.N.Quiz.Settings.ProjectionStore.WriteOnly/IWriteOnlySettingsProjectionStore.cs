using System.Text.Json;
using Poll.N.Quiz.Settings.Domain.ValueObjects;

namespace Poll.N.Quiz.Settings.ProjectionStore.WriteOnly;

public interface IWriteOnlySettingsProjectionStore
{
    public Task SaveProjectionAsync(
        SettingsProjection projection,
        SettingsMetadata settingsMetadata,
        CancellationToken cancellationToken = default);
}
