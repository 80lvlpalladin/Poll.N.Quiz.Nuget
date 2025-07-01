using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Poll.N.Quiz.Settings.ProjectionStore.WriteOnly.Internal;

namespace Poll.N.Quiz.Settings.ProjectionStore.WriteOnly;

public static class ServiceRegistrant
{
    public static IServiceCollection AddWriteOnlySettingsProjectionStore(
        this IServiceCollection services,
        IConfiguration configuration,
        string projectionStoreConnectionString)
    {
        var optionsSection =
            configuration.GetRequiredSection(SettingsProjectionStoreOptions.SectionName);

        return services
            .Configure<SettingsProjectionStoreOptions>(optionsSection)
            .AddSingleton<IWriteOnlyKeyValueStorage>(_ => new RedisWriteOnlyKeyValueStorage(projectionStoreConnectionString))
            .AddSingleton<IWriteOnlySettingsProjectionStore, RedisWriteOnlySettingsProjectionStore>();
    }
}
