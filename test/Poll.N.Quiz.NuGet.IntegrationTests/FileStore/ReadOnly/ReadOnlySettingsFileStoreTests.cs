using Poll.N.Quiz.Settings.FileStore.ReadOnly.Internal;
using Poll.N.Quiz.Settings.Domain.Internal;

namespace Poll.N.Quiz.NuGet.IntegrationTests.FileStore.ReadOnly;

public class ReadOnlySettingsFileStoreTests
{
    private static readonly string TemporarySettingsFilesDirectory =
        Path.Combine(Environment.CurrentDirectory, "TemporarySettingsFiles");

    [Before(Class)]
    public static async Task InitializeFilesAsync()
    {
        const string serviceName1 = "service1";
        const string serviceName2 = "service2";
        const string environmentName1 = "environment1";
        const string environmentName2 = "environment2";
        const string settingsFileName1 = $"{serviceName1}_{environmentName1}.json";
        const string settingsFileName2 = $"{serviceName2}_{environmentName2}.json";
        var jsonData1 = TestSettingsEventFactory.GetExpectedResultSettings(serviceName1, environmentName1);
        var jsonData2 = TestSettingsEventFactory.GetExpectedResultSettings(serviceName2, environmentName2);

        Directory.CreateDirectory(TemporarySettingsFilesDirectory);

        await File.WriteAllTextAsync(Path.Combine(TemporarySettingsFilesDirectory, settingsFileName1), jsonData1);
        await File.WriteAllTextAsync(Path.Combine(TemporarySettingsFilesDirectory, settingsFileName2), jsonData2);

    }

    [After(Class)]
    public static void CleanUp() => Directory.Delete(TemporarySettingsFilesDirectory, true);

    [Test]
    public async Task GetSettingsAsync_JsonData_IfFileExists()
    {
        // Arrange
        var serviceName = "service1";
        var environmentName = "environment1";
        var expectedJsonData = TestSettingsEventFactory.GetExpectedResultSettings(serviceName, environmentName);
        var settingsFileStore = new ReadOnlySettingsFileStore(TemporarySettingsFilesDirectory);

        // Act
        var actualJsonData = await settingsFileStore.GetSettingsAsync(serviceName, environmentName);

        // Assert
        await Assert.That(actualJsonData).IsEqualTo(expectedJsonData);
    }

    [Test]
    public async Task GetSettingsAsync_ThrowsFileNotFoundException_IfFileDoesNotExist()
    {
        // Arrange
        var serviceName = "service3";
        var environmentName = "environment3";
        var settingsFileStore = new ReadOnlySettingsFileStore(TemporarySettingsFilesDirectory);

        // Act
        var act =
            async () => await settingsFileStore.GetSettingsAsync(serviceName, environmentName);

        // Assert
        await Assert.That(act).Throws<FileNotFoundException>();
    }

    [Test]
    public async Task GetAllSettingsMetadata_ReturnsAllSettingsMetadata()
    {
        // Arrange
        var settingsFileStore = new ReadOnlySettingsFileStore(TemporarySettingsFilesDirectory);
        var expectedMetadata = new[]
        {
            ("service1", "environment1"),
            ("service2", "environment2")
        };

        // Act
        var actualMetadata = settingsFileStore.GetAllSettingsMetadata();

        // Assert
        await Assert.That(actualMetadata).IsEquivalentTo(expectedMetadata);
    }

    [Test]
    public async Task Constructor_ThrowsArgumentException_IfPathIsNotFullyQualified()
    {
        // Arrange
        var notFullPath = "TemporarySettingsFiles";

        // Act
        var act = () => new ReadOnlySettingsFileStore(notFullPath);

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }
}
