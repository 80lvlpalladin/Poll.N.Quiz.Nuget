namespace Poll.N.Quiz.Infrastructure.ServiceDiscovery;

public class ConnectionStringResolver
{
    private static readonly Dictionary<AspireResource, string> HardcodedConnectionStrings = new()
    {
        { AspireResource.SettingsApi, "https://localhost:5128" },
        { AspireResource.Api, "https://localhost:5177" }
    };

    public static string GetHardcodedConnectionString(AspireResource resource)
    {
        if (HardcodedConnectionStrings.TryGetValue(resource, out var connectionString))
        {
            return connectionString;
        }

        throw new InvalidOperationException(
            $"No hardcoded connection string found for resource: {resource}");
    }

    public static string GetDotNetConnectionStringFromEnvironment(AspireResource resource)
    {
        var environmentVariableName = $"services__{resource}__https__0";

        var connectionString = Environment.GetEnvironmentVariable(environmentVariableName);

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        throw new InvalidOperationException(
            $"Environment variable '{environmentVariableName}' is not set or is empty.");
    }

}
