using Poll.N.Quiz.Settings.Domain.ValueObjects;

namespace Poll.N.Quiz.Settings.FileStore.ReadOnly.Internal;

internal class ReadOnlySettingsFileStore : IReadOnlySettingsFileStore
{
    private readonly string _settingsFilesFolder;

    internal ReadOnlySettingsFileStore(string settingsFilesFolder)
    {
        if(!Path.IsPathFullyQualified(settingsFilesFolder))
            throw new ArgumentException("The path must be fully qualified.", nameof(settingsFilesFolder));

        _settingsFilesFolder = settingsFilesFolder;
    }

    public Task<string> GetSettingsContentAsync
        (SettingsMetadata settingsMetadata, CancellationToken cancellationToken = default)
    {
        var filePath = CreateSettingsFilePath(settingsMetadata);
        return File.ReadAllTextAsync(filePath, cancellationToken);
    }

    public IEnumerable<SettingsMetadata> GetAllSettingsMetadata
        (CancellationToken cancellationToken = default) =>
        Directory
            .GetFiles(_settingsFilesFolder)
            .Select(ExtractSettingsMetadata)
            .TakeWhile(_ => !cancellationToken.IsCancellationRequested);

    private string CreateSettingsFilePath(SettingsMetadata settingsMetadata) =>
        Path.Combine(
            _settingsFilesFolder,
            $"{settingsMetadata.ServiceName}_{settingsMetadata.EnvironmentName}.json");

    private static SettingsMetadata ExtractSettingsMetadata(string fileName)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var parts = fileNameWithoutExtension.Split('_');
        return new SettingsMetadata(parts[0], parts[1]);
    }
}
