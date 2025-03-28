using Microsoft.Extensions.DependencyInjection;
using Poll.N.Quiz.Settings.FileStore.WriteOnly.Internal;

namespace Poll.N.Quiz.Settings.FileStore.WriteOnly;

public static class ServiceRegistrant
{
    public static IServiceCollection AddWriteOnlySettingsFileStore(
        this IServiceCollection services,
        string settingsFileFolder) =>
        services.AddSingleton<IWriteOnlySettingsFileStore>
            (_ => new WriteOnlySettingsFileStore(settingsFileFolder));
}
