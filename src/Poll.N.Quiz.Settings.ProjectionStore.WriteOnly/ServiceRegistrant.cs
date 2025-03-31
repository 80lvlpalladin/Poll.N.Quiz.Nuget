using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Poll.N.Quiz.Settings.ProjectionStore.WriteOnly.Internal;

namespace Poll.N.Quiz.Settings.ProjectionStore.WriteOnly;

public static class ServiceRegistrant
{
    public static IServiceCollection AddSettingsProjectionStoreOptions
        (this IServiceCollection services, IConfiguration configuration) =>
        services.Configure<SettingsProjectionStoreOptions>(
            configuration.GetRequiredSection(SettingsProjectionStoreOptions.SectionName));

    public static IServiceCollection AddWriteOnlySettingsProjectionStore(
        this IServiceCollection services,
        string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Settings projection connection string cannot be empty");

        return services
            .AddSingleton<IWriteOnlyKeyValueStorage>(_ => new RedisWriteOnlyKeyValueStorage(connectionString))
            .AddSingleton<IWriteOnlySettingsProjectionStore, RedisWriteOnlySettingsProjectionStore>();
    }
}
