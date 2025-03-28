using Poll.N.Quiz.Settings.EventStore.ReadOnly.Internal;
using Poll.N.Quiz.Settings.Domain.Internal;
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
        var serviceName = "service1";
        var environmentName = "environment1";
        var expectedEvents = TestSettingsEventFactory
                .CreateSettingsEvents()
                .Where(se => se.ServiceName == serviceName && se.EnvironmentName == environmentName)
                .ToArray();

        // Act
        var actualEvents =
            await readOnlySettingsUpdateEventStore.GetEventsAsync(serviceName, environmentName);

        // Assert
        await Assert.That(actualEvents).IsNotNull();
        await Assert.That(actualEvents).IsNotEmpty();
        await AssertCollectionsAreEqual(expectedEvents, actualEvents);
    }

    [Test]
    public async Task GetEventsAsync_ReturnsEmptyCollection_WhenServiceNameIsMissing()
    {
        // Arrange
        var readOnlySettingsUpdateEventStore =
            new MongoReadOnlySettingsEventStore(_seededMongoDbFixture.MongoClient!);
        var serviceName = string.Empty;
        var environmentName = "environment1";

        // Act
        var actualEvents =
            await readOnlySettingsUpdateEventStore.GetEventsAsync(serviceName, environmentName);

        // Assert
        await Assert.That(actualEvents).IsNotNull();
        await Assert.That(actualEvents).IsEmpty();
    }

    [Test]
    public async Task GetEventsAsync_ReturnsEmptyCollection_WhenEnvironmentNameIsMissing()
    {
        // Arrange
        var readOnlySettingsUpdateEventStore =
            new MongoReadOnlySettingsEventStore(_seededMongoDbFixture.MongoClient!);
        var serviceName = "service1";
        var environmentName = string.Empty;

        // Act
        var actualEvents =
            await readOnlySettingsUpdateEventStore.GetEventsAsync(serviceName, environmentName);

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
        var actualEvents = await readOnlySettingsUpdateEventStore.GetAllEventsAsync();

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
