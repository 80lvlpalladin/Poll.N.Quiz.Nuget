using Refit;
using Poll.N.Quiz.Infrastructure.Clients.Payloads;

namespace Poll.N.Quiz.Infrastructure.Clients;


public interface ISettingsApiClient
{
    [Get("/api/v1/settings/metadata")]
    Task<GetAllSettingsMetadataResponse> GetAllMetadataAsync();

    [Get("/api/v1/settings/metadata/{serviceName}")]
    Task<SettingsMetadataResponse> GetMetadataAsync(string serviceName);

    [Post("/api/v1/settings/reload-projection")]
    Task ReloadProjectionAsync([Body] ReloadProjectionRequest request);

    [Post("/api/v1/settings")]
    Task CreateSettingsAsync([Body] CreateSettingsRequest request);

    [Patch("/api/v1/settings")]
    Task UpdateSettingsAsync([Body] UpdateSettingsRequest request);

    [Get("/api/v1/settings/{serviceName}/{environmentName}")]
    Task<GetSettingsContentResponse> GetSettingsContentAsync(string serviceName, string environmentName);
}


