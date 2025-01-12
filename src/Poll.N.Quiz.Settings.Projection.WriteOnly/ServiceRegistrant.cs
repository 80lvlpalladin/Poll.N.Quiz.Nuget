using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Poll.N.Quiz.Settings.Projection.WriteOnly.Internal;

namespace Poll.N.Quiz.Settings.Projection.WriteOnly;

public static class ServiceRegistrant
{
    public static IServiceCollection AddWriteOnlySettingsProjection
        (this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString =
            configuration.GetConnectionString("Redis");

        if (string.IsNullOrWhiteSpace(redisConnectionString))
            throw new ArgumentException("Redis connection string was not found in configuration");

        return services
            .Configure<SettingsProjectionOptions>(configuration.GetRequiredSection(SettingsProjectionOptions.SectionName))
            .AddSingleton<IWriteOnlyKeyValueStorage>(_ => new RedisWriteOnlyStorage(redisConnectionString))
            .AddSingleton<IWriteOnlySettingsProjection, WriteOnlySettingsProjection>();
    }
}
