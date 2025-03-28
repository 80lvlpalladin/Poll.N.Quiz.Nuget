using Microsoft.Extensions.Configuration;

namespace Poll.N.Quiz.Aspire;

public static class ConnectionStringResolver
{
    public static string GetSettingsEventStoreConnectionString(this IConfiguration configuration) =>
        configuration.GetConnectionStringInternal(ResourceNames.SettingsEventStore);

    public static string GetSettingsProjectionConnectionString(this IConfiguration configuration) =>
        configuration.GetConnectionStringInternal(ResourceNames.SettingsProjection);

    public static string GetSettingsEventQueueConnectionString(this IConfiguration configuration) =>
        configuration.GetConnectionStringInternal(ResourceNames.SettingsEventQueue);

    private static string GetConnectionStringInternal
        (this IConfiguration configuration, string connectionStringSectionName)
    {
        var connectionString = configuration.GetConnectionString(connectionStringSectionName);

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException($"ConnectionStrings__{connectionStringSectionName}");

        return connectionString;

    }
}

