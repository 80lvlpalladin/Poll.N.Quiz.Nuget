using Microsoft.Extensions.Options;
using Moq;
using Poll.N.Quiz.Settings.Domain.ValueObjects;
using Poll.N.Quiz.Settings.ProjectionStore.WriteOnly.Internal;

namespace Poll.N.Quiz.Settings.Projection.WriteOnly.UnitTests.Internal;

public class RedisWriteOnlySettingsProjectionTests
{
    [Test]
    public async Task SaveProjectionAsync_SavesProjection_ToWriteOnlyStorage()
    {
        // Arrange
        ushort expectedExpirationTimeHours = 1;
        var serviceName = "service1";
        var environmentName = "environment1";
        var settingsMetadata = new SettingsMetadata(serviceName, environmentName);
        var expectedStorageKey = $"{serviceName}__{environmentName}";
        uint expectedLastUpdatedTimeStamp = 123124132;
        uint expectedVersion = 0;
        var expectedSettings =
            $$"""
              {
              "LastUpdatedTimeStamp": {{expectedLastUpdatedTimeStamp}},
              "Settings": {"key1": "value1"}
              }
              """;
        var expectedProjectionModel = new SettingsProjection(expectedSettings, expectedLastUpdatedTimeStamp, expectedVersion);
        var writeOnlyStorageMock = new Mock<IWriteOnlyKeyValueStorage>();
        writeOnlyStorageMock.Setup(storage => storage.SetAsync(
                It.Is<string>(str => str == expectedStorageKey),
                It.Is<SettingsProjection>(pm => pm.Equals(expectedProjectionModel)),
                It.Is<TimeSpan?>(ts => ts == TimeSpan.FromHours(expectedExpirationTimeHours))))
            .Returns(Task.CompletedTask);

        var settingsProjectionOptions = new SettingsProjectionStoreOptions()
        {
            ExpirationTimeHours = expectedExpirationTimeHours
        };
        var settingsProjectionRepository = new RedisWriteOnlySettingsProjectionStore
            (writeOnlyStorageMock.Object, Options.Create(settingsProjectionOptions));


        // Act
        await settingsProjectionRepository.SaveProjectionAsync(expectedProjectionModel, settingsMetadata);

        // Assert
        writeOnlyStorageMock.Verify(storage => storage.SetAsync(
            It.Is<string>(str => str == expectedStorageKey),
            It.Is<SettingsProjection>(pm => pm.Equals(expectedProjectionModel)),
            It.Is<TimeSpan?>(ts => ts == TimeSpan.FromHours(expectedExpirationTimeHours))), Times.Once);
    }
}
