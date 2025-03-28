using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Poll.N.Quiz.Settings.Projection.WriteOnly.Internal;

namespace Poll.N.Quiz.Settings.Projection.WriteOnly;

public static class ServiceRegistrant
{
    public static IServiceCollection AddSettingsProjectionOptions
        (this IServiceCollection services, IConfiguration configuration) =>
        services.Configure<SettingsProjectionOptions>(
            configuration.GetRequiredSection(SettingsProjectionOptions.SectionName));

    public static IServiceCollection AddWriteOnlySettingsProjection(
        this IServiceCollection services,
        string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Settings projection connection string cannot be empty");

        return services
            .AddSingleton<IWriteOnlyKeyValueStorage>(_ => new RedisWriteOnlyKeyValueStorage(connectionString))
            .AddSingleton<IWriteOnlySettingsProjection, RedisWriteOnlySettingsProjection>();
    }
}
