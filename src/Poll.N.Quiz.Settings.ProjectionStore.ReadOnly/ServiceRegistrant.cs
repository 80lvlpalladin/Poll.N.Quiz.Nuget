using Microsoft.Extensions.DependencyInjection;
using Poll.N.Quiz.Settings.ProjectionStore.ReadOnly.Internal;

namespace Poll.N.Quiz.Settings.ProjectionStore.ReadOnly;

public static class ServiceRegistrant
{
    public static IServiceCollection AddReadOnlySettingsProjectionStore
        (this IServiceCollection services, string connectionString) =>
        services
            .AddSingleton<IReadOnlyKeyValueStorage>(_ => new RedisReadOnlyKeyValueStorage(connectionString))
            .AddSingleton<IReadOnlySettingsProjectionStore, RedisReadOnlySettingsProjectionStore>();
}
