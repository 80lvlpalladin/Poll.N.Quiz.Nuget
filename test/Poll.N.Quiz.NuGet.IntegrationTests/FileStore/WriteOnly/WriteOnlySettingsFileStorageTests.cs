using Poll.N.Quiz.Settings.Domain.ValueObjects;
using Poll.N.Quiz.Settings.FileStore.ReadOnly.Internal;
using Poll.N.Quiz.Settings.FileStore.WriteOnly.Internal;

namespace Poll.N.Quiz.NuGet.IntegrationTests.FileStore.WriteOnly;

public class WriteOnlySettingsFileStorageTests
{
    private static readonly string TemporarySettingsFilesDirectory =
        Path.Combine(Environment.CurrentDirectory, "TemporarySettingsFiles");

    [Before(Class)]
    public static void Initialize() => Directory.CreateDirectory(TemporarySettingsFilesDirectory);

    [After(Class)]
    public static void CleanUp() => Directory.Delete(TemporarySettingsFilesDirectory, true);

    [Test]
    public async Task SaveAsync_IsThreadSafe()
    {
        //Arrange
        var writeOnlySettingsFileStore = new WriteOnlySettingsFileStore(TemporarySettingsFilesDirectory);
        var readWriteSettingsFileStore = new ReadOnlySettingsFileStore(TemporarySettingsFilesDirectory);
        var settingsMetadata = new SettingsMetadata("service1", "environment1");
        string[] settingsFilesContents = [
            "version1",
            "version2",
            "version3",
            "version4",
            "version5"
        ];
        var expectedSavedSettings = settingsFilesContents.Last();


        //Act
        await Task.WhenAll(settingsFilesContents.Select(settingsFileContent =>
            writeOnlySettingsFileStore.SaveAsync(settingsMetadata, settingsFileContent)));

        //Assert
        var actualSavedSettings =
            await readWriteSettingsFileStore.GetSettingsContentAsync(settingsMetadata);

        await Assert.That(actualSavedSettings).IsEqualTo(expectedSavedSettings);
    }

    [Test]
    public async Task SaveAsync_ThrowsIOException_IfFileIsLocked()
    {
        //Arrange
        var writeOnlySettingsFileStore = new WriteOnlySettingsFileStore(TemporarySettingsFilesDirectory);
        const string serviceName = "service2";
        const string environmentName = "environment2";
        var settingsMetadata = new SettingsMetadata(serviceName, environmentName);
        var filePath = Path.Combine(TemporarySettingsFilesDirectory, $"{serviceName}_{environmentName}.json");
        await using var fileLock = new FileStream(
            filePath,
            FileMode.OpenOrCreate,
            FileAccess.ReadWrite,
            FileShare.None // No other process can write or read
        );

        //Act
        var act =
            async () => await writeOnlySettingsFileStore.SaveAsync(settingsMetadata, "version1");

        // Assert
        await Assert.ThrowsAsync<IOException>(act);
    }

    [Test]
    public void Constructor_ThrowsArgumentException_IfPathIsNotFullyQualified()
    {
        // Arrange
        var notFullPath = "TemporarySettingsFiles";

        // Act
        var act = () =>
        {
            _ = new WriteOnlySettingsFileStore(notFullPath);
        };

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

}
