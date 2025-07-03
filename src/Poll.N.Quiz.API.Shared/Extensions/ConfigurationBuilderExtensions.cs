using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Poll.N.Quiz.Infrastructure.Clients;

namespace Poll.N.Quiz.API.Shared.Extensions;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddConfigurationFromSettingsApi(
        this IConfigurationBuilder builder,
        string serviceName,
        string environmentName,
        ISettingsApiClient settingsApiClient) =>
        builder.Add(new WebConfigurationSource(
            serviceName,
            environmentName,
            settingsApiClient));

    private sealed class WebConfigurationSource
        (string serviceName, string environmentName, ISettingsApiClient settingsApiClient) : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder) =>
            new WebConfigurationProvider(serviceName, environmentName, settingsApiClient);
    }

    private sealed class WebConfigurationProvider
        (string serviceName, string environmentName, ISettingsApiClient settingsApiClient) : ConfigurationProvider
    {

        public override void Load()
        {
            var settingsJson =
                settingsApiClient.GetSettingsContentAsync(serviceName, environmentName).GetAwaiter().GetResult();

            if(settingsJson is null || string.IsNullOrWhiteSpace(settingsJson.JsonData))
            {
                throw new InvalidOperationException(
                    $"Settings for service '{serviceName}' in environment '{environmentName}' are not found or empty.");
            }

            Data = JsonSerializer.Deserialize<Dictionary<string, string?>>(settingsJson.JsonData, JsonSerializerOptions.Default)
                   ?? new Dictionary<string, string?>();
        }
    }
}
