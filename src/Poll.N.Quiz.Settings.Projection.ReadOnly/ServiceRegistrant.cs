using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Poll.N.Quiz.Settings.Projection.ReadOnly.Internal;

namespace Poll.N.Quiz.Settings.Projection.ReadOnly;

public static class ServiceRegistrant
{
    public static IServiceCollection AddReadOnlySettingsProjection
        (this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis");

        if (string.IsNullOrWhiteSpace(redisConnectionString))
            throw new ArgumentException("Redis connection string is not set in the configuration");

        return services
            .AddSingleton<RedisReadOnlyStorage>(_ => new RedisReadOnlyStorage(redisConnectionString))
            .AddSingleton<IReadOnlySettingsProjection, ReadOnlySettingsProjection>();
    }
}
