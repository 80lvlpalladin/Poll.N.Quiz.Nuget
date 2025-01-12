using Microsoft.Extensions.Options;
using Moq;
using Poll.N.Quiz.Settings.Projection.WriteOnly.Internal;

namespace Poll.N.Quiz.Settings.Projection.WriteOnly.UnitTests.Internal;

public class WriteOnlySettingsProjectionTests
{
    [Test]
    public async Task SaveProjectionAsync_SavesProjection_ToWriteOnlyStorage()
    {
        // Arrange
        ushort expectedExpirationTimeHours = 1;
        var serviceName = "service1";
        var environmentName = "environment1";
        var expectedStorageKey = $"{serviceName}__{environmentName}";
        uint expectedLastUpdatedTimeStamp = 123124132;
        var expectedSettings =
            $$"""
              {
              "LastUpdatedTimeStamp": {{expectedLastUpdatedTimeStamp}},
              "Settings": {"key1": "value1"}
              }
              """;
        var expectedProjectionModel = new ProjectionModel(expectedLastUpdatedTimeStamp, expectedSettings);
        var writeOnlyStorageMock = new Mock<IWriteOnlyKeyValueStorage>();
        writeOnlyStorageMock.Setup(storage => storage.SetAsync(
                It.Is<string>(str => str == expectedStorageKey),
                It.Is<ProjectionModel>(pm => pm.Equals(expectedProjectionModel)),
                It.Is<TimeSpan?>(ts => ts == TimeSpan.FromHours(expectedExpirationTimeHours))))
            .Returns(Task.CompletedTask);

        var settingsProjectionOptions = new SettingsProjectionOptions
        {
            ExpirationTimeHours = expectedExpirationTimeHours
        };
        var settingsProjectionRepository = new WriteOnlySettingsProjection
            (writeOnlyStorageMock.Object, Options.Create(settingsProjectionOptions));


        // Act
        await settingsProjectionRepository.SaveProjectionAsync(
            expectedLastUpdatedTimeStamp, serviceName, environmentName, expectedSettings);

        // Assert
        writeOnlyStorageMock.Verify(storage => storage.SetAsync(
            It.Is<string>(str => str == expectedStorageKey),
            It.Is<ProjectionModel>(pm => pm.Equals(expectedProjectionModel)),
            It.Is<TimeSpan?>(ts => ts == TimeSpan.FromHours(expectedExpirationTimeHours))), Times.Once);
    }
}
