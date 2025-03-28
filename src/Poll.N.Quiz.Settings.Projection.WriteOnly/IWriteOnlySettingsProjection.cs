using System.Text.Json;
using Poll.N.Quiz.Settings.Domain.ValueObjects;

namespace Poll.N.Quiz.Settings.Projection.WriteOnly;

public interface IWriteOnlySettingsProjection
{
    public Task SaveProjectionAsync(
        SettingsProjection projection,
        SettingsMetadata settingsMetadata,
        CancellationToken cancellationToken = default);
}
