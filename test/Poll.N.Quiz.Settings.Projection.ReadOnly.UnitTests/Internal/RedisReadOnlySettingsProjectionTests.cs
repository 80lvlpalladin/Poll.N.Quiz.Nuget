using Moq;
using Poll.N.Quiz.Settings.Domain.ValueObjects;
using Poll.N.Quiz.Settings.ProjectionStore.ReadOnly.Internal;
using Assert = TUnit.Assertions.Assert;

namespace Poll.N.Quiz.Settings.Projection.ReadOnly.UnitTests.Internal;

public class RedisReadOnlySettingsProjectionTests
{
    [Test]
    public async Task GetAllSettingsMetadataAsync_ReturnsMetadata_FromReadOnlyStorage()
    {
        // Arrange
        var redisReadOnlyStorageMock = new Mock<IReadOnlyKeyValueStorage>();
        redisReadOnlyStorageMock
            .Setup(storage =>
                storage.ListAllKeysAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                "service1__environment1",
                "service1__environment2",
                "service2__environment3"]);


        var settingsProjectionRepository = new RedisReadOnlySettingsProjectionStore(redisReadOnlyStorageMock.Object);
        var expectedMetadata = new[]
        {
            new SettingsMetadata("service1", "environment1"),
            new SettingsMetadata("service1", "environment2"),
            new SettingsMetadata("service2", "environment3")
        };

        // Act
        var actualMetadata = (SettingsMetadata[])
            await settingsProjectionRepository.GetAllSettingsMetadataAsync();

        // Assert
        await Assert.That(actualMetadata).IsNotNull();
        await Assert.That(actualMetadata).IsNotEmpty();
        redisReadOnlyStorageMock.Verify(storage =>
            storage.ListAllKeysAsync(It.IsAny<CancellationToken>()), Times.Once);
        await Assert.That(actualMetadata.Length).IsEqualTo(expectedMetadata.Length);

        for (var i = 0; i < expectedMetadata.Length; i++)
        {
            await Assert.That(actualMetadata[i]).IsEquivalentTo(expectedMetadata[i]);
        }
    }

    [Test]
    public async Task GetAllSettingsMetadataAsync_ReturnsEmptyCollection_IfReadOnlyStorageEmpty()
    {
        // Arrange
        var redisReadOnlyStorageMock = new Mock<IReadOnlyKeyValueStorage>();
        redisReadOnlyStorageMock
            .Setup(storage =>
                storage.ListAllKeysAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var settingsProjectionRepository = new RedisReadOnlySettingsProjectionStore(redisReadOnlyStorageMock.Object);

        // Act
        var actualMetadata =
            await settingsProjectionRepository.GetAllSettingsMetadataAsync();

        // Assert
        await Assert.That(actualMetadata).IsNotNull();
        await Assert.That(actualMetadata).IsEmpty();
    }

    [Test]
    public async Task GetProjectionAsync_ReturnsProjection_FromReadOnlyStorage()
    {
        //Arrange
        var serviceName = "service1";
        var environmentName = "environment1";
        var settingsMetadata = new SettingsMetadata(serviceName, environmentName);
        var expectedKey = $"{serviceName}__{environmentName}";
        uint expectedLastUpdatedTimeStamp = 123124132;
        uint expectedVersion = 0;
        var expectedSettings =
            $$"""
              {
              "LastUpdatedTimeStamp": {{expectedLastUpdatedTimeStamp}},
              "Settings": {"key1": "value1"}
              }
              """.Replace("\n", "").Replace(" ", "");
        var expectedValue =
            new SettingsProjection(expectedSettings, expectedLastUpdatedTimeStamp, expectedVersion);

        var redisReadOnlyStorageMock = new Mock<IReadOnlyKeyValueStorage>();
        redisReadOnlyStorageMock
            .Setup(storage => storage.GetAsync<SettingsProjection>(expectedKey))
            .ReturnsAsync(expectedValue);

        var settingsProjection =
            new RedisReadOnlySettingsProjectionStore(redisReadOnlyStorageMock.Object);

        //Act
        var actualProjection =
            await settingsProjection.GetAsync(settingsMetadata);

        //Assert
        await Assert.That(actualProjection).IsNotNull();
        await Assert.That(actualProjection!.LastUpdatedTimestamp).IsEqualTo(expectedLastUpdatedTimeStamp);
        await Assert.That(actualProjection!.JsonData).IsEqualTo(expectedSettings);
        await Assert.That(actualProjection!.Version).IsEqualTo(expectedVersion);
    }

    [Test]
    public async Task GetProjectionAsync_ReturnsNull_IfReadOnlyKeyValueStorageIsEmpty()
    {
        //Arrange
        var serviceName = "service1";
        var environmentName = "environment1";
        var settingsMetadata = new SettingsMetadata(serviceName, environmentName);
        var expectedKey = $"{serviceName}__{environmentName}";
        SettingsProjection? expectedValue = null;
        var redisReadOnlyStorageMock = new Mock<IReadOnlyKeyValueStorage>();
        redisReadOnlyStorageMock
            .Setup(storage => storage.GetAsync<SettingsProjection>(expectedKey))
            .ReturnsAsync(expectedValue);
        var settingsProjection = new RedisReadOnlySettingsProjectionStore(redisReadOnlyStorageMock.Object);

        //Act
        var actualProjection =
            await settingsProjection.GetAsync(settingsMetadata);

        //Assert
        await Assert.That(actualProjection).IsNull();
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task IsEmptyAsync_ReturnsSameResult_AsReadOnlyStorage(bool readOnlyStorageEmpty)
    {
        // Arrange
        var redisReadOnlyStorageMock = new Mock<IReadOnlyKeyValueStorage>();
        redisReadOnlyStorageMock
            .Setup(storage => storage.IsEmptyAsync())
            .ReturnsAsync(readOnlyStorageEmpty);
        var settingsProjection = new RedisReadOnlySettingsProjectionStore(redisReadOnlyStorageMock.Object);

        // Act
        var projectionEmpty = await settingsProjection.IsEmptyAsync();

        // Assert
        await Assert.That(projectionEmpty).IsEqualTo(readOnlyStorageEmpty);
    }
}
