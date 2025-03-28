using Microsoft.Extensions.DependencyInjection;
using Poll.N.Quiz.Settings.Projection.ReadOnly.Internal;

namespace Poll.N.Quiz.Settings.Projection.ReadOnly;

public static class ServiceRegistrant
{
    public static IServiceCollection AddReadOnlySettingsProjection
        (this IServiceCollection services, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Settings projection connection string cannot be empty");

        return services
            .AddSingleton<IReadOnlyKeyValueStorage>(_ => new RedisReadOnlyKeyValueStorage(connectionString))
            .AddSingleton<IReadOnlySettingsProjection, RedisReadOnlySettingsProjection>();
    }
}
