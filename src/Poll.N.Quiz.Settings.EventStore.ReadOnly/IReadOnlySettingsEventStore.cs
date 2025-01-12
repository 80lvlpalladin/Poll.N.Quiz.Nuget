using Poll.N.Quiz.Settings.Messaging.Contracts;

namespace Poll.N.Quiz.Settings.EventStore.ReadOnly;

public interface IReadOnlySettingsEventStore
{
    public Task<SettingsEvent[]> GetEventsAsync
        (string serviceName, string environmentName, CancellationToken cancellationToken = default);

    public Task<SettingsEvent[]> GetAllEventsAsync
        (CancellationToken cancellationToken = default);
}
