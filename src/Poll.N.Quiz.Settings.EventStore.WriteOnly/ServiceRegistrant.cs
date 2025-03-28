using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Poll.N.Quiz.Settings.EventStore.WriteOnly.Internal;

namespace Poll.N.Quiz.Settings.EventStore.WriteOnly;

public static class ServiceRegistrant
{
    public static IServiceCollection AddWriteOnlySettingsEventStore
        (this IServiceCollection services, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Settings event store connection string cannot be empty");

        return services
            .AddSingleton<IMongoClient>(_ => new MongoClient(connectionString))
            .AddSingleton<IWriteOnlySettingsEventStore, MongoWriteOnlySettingsEventStore>();
    }
}
