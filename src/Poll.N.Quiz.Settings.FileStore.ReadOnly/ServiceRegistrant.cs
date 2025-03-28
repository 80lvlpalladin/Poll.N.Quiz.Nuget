using Microsoft.Extensions.DependencyInjection;
using Poll.N.Quiz.Settings.FileStore.ReadOnly.Internal;

namespace Poll.N.Quiz.Settings.FileStore.ReadOnly;

public static class ServiceRegistrant
{
    public static IServiceCollection AddReadOnlySettingsFileStore(
        this IServiceCollection services,
        string settingsFileFolder) =>
        services.AddSingleton<IReadOnlySettingsFileStore>
            (_ => new ReadOnlySettingsFileStore(settingsFileFolder));
}
