using Poll.N.Quiz.Settings.Domain.ValueObjects;

namespace Poll.N.Quiz.Settings.EventStore.ReadOnly;

public interface IReadOnlySettingsEventStore
{
    public Task<SettingsEvent[]> GetAsync
        (SettingsMetadata settingsMetadata, CancellationToken cancellationToken = default);

    public Task<SettingsEvent[]> GetAllAsync
        (CancellationToken cancellationToken = default);
}
