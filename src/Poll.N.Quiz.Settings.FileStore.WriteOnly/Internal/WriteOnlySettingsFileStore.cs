using Poll.N.Quiz.Settings.Domain.ValueObjects;

namespace Poll.N.Quiz.Settings.FileStore.WriteOnly.Internal;

internal class WriteOnlySettingsFileStore : IWriteOnlySettingsFileStore, IDisposable
{
    private readonly string _settingsFilesFolder;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    internal WriteOnlySettingsFileStore(string settingsFilesFolder)
    {
        if(!Path.IsPathFullyQualified(settingsFilesFolder))
            throw new ArgumentException("The path must be fully qualified.", nameof(settingsFilesFolder));

        _settingsFilesFolder = settingsFilesFolder;
    }

    public async Task SaveAsync(
        SettingsMetadata settingsMetadata,
        string jsonData,
        CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        var filePath = CreateSettingsFilePath(settingsMetadata);
        await File.WriteAllTextAsync(filePath, jsonData, cancellationToken);

        _semaphore.Release();
    }

    private string CreateSettingsFilePath(SettingsMetadata settingsMetadata) =>
        Path.Combine(
            _settingsFilesFolder,
            $"{settingsMetadata.ServiceName}_{settingsMetadata.EnvironmentName}.json");

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _semaphore.Dispose();
    }
}
