using Poll.N.Quiz.Settings.Domain;
using Poll.N.Quiz.Settings.EventStore.ReadOnly.Internal;
using Poll.N.Quiz.Settings.Domain.ValueObjects;

namespace Poll.N.Quiz.NuGet.IntegrationTests.EventStore.ReadOnly;

public class MongoReadOnlySettingsEventStoreTests()
{
    private static readonly SeededMongoDbFixture _seededMongoDbFixture = new();

    [Before(Class)]
    public static Task InitializeFixtureAsync() => _seededMongoDbFixture.InitializeAsync();

    [After(Class)]
    public static ValueTask DisposeFixtureAsync() => _seededMongoDbFixture.DisposeAsync();

    [Test]
    public async Task GetEventsAsync_ReturnsEvents_ForServiceAndEnvironment()
    {
        // Arrange
        var readOnlySettingsUpdateEventStore =
            new MongoReadOnlySettingsEventStore(_seededMongoDbFixture.MongoClient!);
        var settingsMetadata =
            new SettingsMetadata("service1", "environment1");
        var expectedEvents = TestSettingsEventFactory
                .CreateSettingsEvents()
                .Where(se => se.Metadata == settingsMetadata)
                .ToArray();

        // Act
        var actualEvents =
            await readOnlySettingsUpdateEventStore.GetAsync(settingsMetadata);

        // Assert
        await Assert.That(actualEvents).IsNotNull();
        await Assert.That(actualEvents).IsNotEmpty();
        await AssertCollectionsAreEqual(expectedEvents, actualEvents);
    }

    [Test]
    [Arguments("", "environment1")]
    [Arguments("service1", "")]
    public async Task GetEventsAsync_ReturnsEmptyCollection_WhenServiceOrEnvironmentNameIsMissing
        (string serviceName, string environmentName)
    {
        // Arrange
        var readOnlySettingsUpdateEventStore =
            new MongoReadOnlySettingsEventStore(_seededMongoDbFixture.MongoClient!);
        var settingsMetadata =
            new SettingsMetadata(serviceName, environmentName);

        // Act
        var actualEvents =
            await readOnlySettingsUpdateEventStore.GetAsync(settingsMetadata);

        // Assert
        await Assert.That(actualEvents).IsNotNull();
        await Assert.That(actualEvents).IsEmpty();
    }

    [Test]
    public async Task GetAllEventsAsync_ReturnsAllEvents()
    {
        // Arrange
        var readOnlySettingsUpdateEventStore = new MongoReadOnlySettingsEventStore(_seededMongoDbFixture.MongoClient!);
        var expectedEvents = TestSettingsEventFactory.CreateSettingsEvents();

        // Act
        var actualEvents = await readOnlySettingsUpdateEventStore.GetAllAsync();

        // Assert
        await Assert.That(actualEvents).IsNotNull();
        await Assert.That(actualEvents).IsNotEmpty();
        await AssertCollectionsAreEqual(expectedEvents, actualEvents);
    }

    private async Task AssertCollectionsAreEqual
        (IEnumerable<SettingsEvent> expected, IEnumerable<SettingsEvent> actual)
    {
        var orderedExpected = expected.OrderBy(e => e.TimeStamp).ToArray();
        var orderedActual = actual.OrderBy(e => e.TimeStamp).ToArray();

        await Assert.That(orderedExpected.Length).IsEqualTo(orderedActual.Length);

        for (var i = 0; i < orderedExpected.Length; i++)
        {
            await Assert.That(orderedExpected[i]).IsEqualTo(orderedActual[i]);
        }
    }
}
