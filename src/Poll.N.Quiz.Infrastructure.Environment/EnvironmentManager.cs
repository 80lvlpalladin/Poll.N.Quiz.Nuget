using System.Diagnostics.CodeAnalysis;
using Poll.N.Quiz.Infrastructure.Clients;

namespace Poll.N.Quiz.Infrastructure.Environment;

public sealed class EnvironmentManager
    (ISettingsApiClient settingsApiClient, string serviceName)
{
    const string EnvironmentVariableName = "ASPNETCORE_ENVIRONMENT";

    private string? _currentEnvironment;

    private string[]? _supportedEnvironments;

    private readonly string[] _developmentEnvironmentMonikers =
    [
        "Development",
        "Dev",
        "Local",
        "Test"
    ];


    public async ValueTask<string> GetCurrentEnvironmentAsync()
    {
        if(_currentEnvironment is not null && _supportedEnvironments is not null)
            return _currentEnvironment;

        var settingsMetadata =
            await settingsApiClient.GetMetadataAsync(serviceName);

        if(settingsMetadata.EnvironmentNames is null || settingsMetadata.EnvironmentNames.Length == 0)
            throw new InvalidOperationException(
                $"No supported environments found for service {serviceName}");

        var currentEnvironment =
            System.Environment.GetEnvironmentVariable(EnvironmentVariableName);

        if(string.IsNullOrWhiteSpace(currentEnvironment))
            throw new InvalidOperationException(
                $"Environment variable '{EnvironmentVariableName}' is not set or is empty.");


        if (!settingsMetadata.EnvironmentNames.Any(env => EnvironmentNameEquals(env, currentEnvironment)))
            throw new InvalidEnvironmentException(currentEnvironment, settingsMetadata.EnvironmentNames);

        _currentEnvironment = currentEnvironment;
        _supportedEnvironments = settingsMetadata.EnvironmentNames;

        return _currentEnvironment;
    }

    public async ValueTask<bool> IsDevelopmentAsync()
    {
        var currentEnvironment = await GetCurrentEnvironmentAsync();

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var environmentName in _developmentEnvironmentMonikers)
        {
            if(EnvironmentNameEquals(environmentName, currentEnvironment))
            {
                return true;
            }
        }

        return false;
    }

    private static bool EnvironmentNameEquals(string expected, string actual) =>
        string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase);

    private sealed class InvalidEnvironmentException
        (string currentEnvironment, string[] supportedEnvironments) : Exception(
        $"Environment {currentEnvironment} is invalid, Valid environments are " +
        $"{string.Join(" ", supportedEnvironments)}");
}


