using System.Text.Json;

namespace Poll.N.Quiz.Settings.Projection.WriteOnly;

public interface IWriteOnlySettingsProjection
{
    public Task SaveProjectionAsync(
        uint timeStamp,
        string serviceName,
        string environmentName,
        string settingsJson,
        CancellationToken cancellationToken = default);
}
