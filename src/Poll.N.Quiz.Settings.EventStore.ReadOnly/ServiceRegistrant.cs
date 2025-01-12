using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Poll.N.Quiz.Settings.EventStore.ReadOnly.Internal;

namespace Poll.N.Quiz.Settings.EventStore.ReadOnly;

public static class ServiceRegistrant
{
    public static IServiceCollection AddReadOnlySettingsUpdateEventStore
        (this IServiceCollection services, IConfiguration configuration)
    {
        var mongoConnectionString = configuration.GetConnectionString("Mongo");

        if (string.IsNullOrWhiteSpace(mongoConnectionString))
            throw new ArgumentException("Mongo connection string is not set in the configuration");

        return services
            .AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString))
            .AddSingleton<IReadOnlySettingsEventStore, MongoReadOnlySettingsEventStore>();
    }

    public static IServiceCollection AddReadOnlySettingsUpdateEventStore
        (this IServiceCollection services, string mongoConnectionString)
    {
        if (string.IsNullOrWhiteSpace(mongoConnectionString))
            throw new ArgumentException("Redis connection string cannot be empty");

        return services
            .AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString))
            .AddSingleton<IReadOnlySettingsEventStore, MongoReadOnlySettingsEventStore>();
    }
}
