using Poll.N.Quiz.Settings.Domain.ValueObjects;

namespace Poll.N.Quiz.Settings.EventStore.WriteOnly;

public interface IWriteOnlySettingsEventStore
{
    public Task<bool> SaveAsync(SettingsEvent @event, CancellationToken cancellationToken = default);
}
