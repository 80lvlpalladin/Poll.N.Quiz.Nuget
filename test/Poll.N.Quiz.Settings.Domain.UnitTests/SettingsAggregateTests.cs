using ErrorOr;
using Poll.N.Quiz.Settings.Domain.ValueObjects;


namespace Poll.N.Quiz.Settings.Domain.UnitTests;

public class SettingsAggregateTests
{
    [Test]
    public async Task InstanceCreation_FromSettingsCreateEvent()
    {
        // Arrange
        var settingsCreateEvent = TestSettingsEventFactory.CreateSettingsCreateEvent();
        var expectedCurrentProjection = new SettingsProjection(
                settingsCreateEvent.JsonData,
                settingsCreateEvent.TimeStamp,
                settingsCreateEvent.Version);

        // Act
        var settingsAggregate = new SettingsAggregate(settingsCreateEvent.Metadata);
        var applyResult = settingsAggregate.TryApplyEvent(settingsCreateEvent, out _);

        // Assert
        await Assert.That(applyResult).IsTrue();
        await Assert.That(settingsAggregate.Metadata).IsEqualTo(settingsCreateEvent.Metadata);
        await Assert.That(settingsAggregate.CurrentProjection).IsEqualTo(expectedCurrentProjection);
    }

    [Test]
    public async Task InstanceCreation_FromSettingsUpdateEvent()
    {
        //Arrange
        var settingsEvents = TestSettingsEventFactory
            .CreateSettingsEvents()
            .Where(se => se.Metadata is {ServiceName: "service1", EnvironmentName:"environment1" })
            .ToArray();
        var settingsUpdateEvent = settingsEvents[1];
        var settingsCreateEvent = settingsEvents[0];
        var initialProjection = new SettingsProjection(
            settingsCreateEvent.JsonData,
            settingsCreateEvent.TimeStamp,
            settingsCreateEvent.Version);
        var expectedResultSettingsJson =
            SettingsAggregate.ApplyJsonPatch(initialProjection.JsonData, settingsUpdateEvent.JsonData).Value;
        var expectedCurrentProjection = new SettingsProjection(
            expectedResultSettingsJson,
            settingsUpdateEvent.TimeStamp,
            settingsUpdateEvent.Version);

        // Act
        var settingsAggregate = new SettingsAggregate(settingsUpdateEvent.Metadata, initialProjection);
        var applyResult = settingsAggregate.TryApplyEvent(settingsUpdateEvent, out _);

        // Assert
        await Assert.That(applyResult).IsTrue();
        await Assert.That(settingsAggregate.Metadata).IsEqualTo(settingsCreateEvent.Metadata);
        await Assert.That(settingsAggregate.CurrentProjection).IsEqualTo(expectedCurrentProjection);
    }

    [Test]
    [Arguments("service1","environment1")]
    [Arguments("service2", "environment2")]
    public async Task ApplyEvent_WhenApplyingSeriesOfEvents_CreatesValidResultProjection(string serviceName, string environmentName)
    {
        // Arrange
        var settingsEvents = TestSettingsEventFactory
            .CreateSettingsEvents()
            .Where(se =>
                se.Metadata.ServiceName == serviceName &&
                se.Metadata.EnvironmentName == environmentName)
            .ToArray();
        var expectedResultSettingsJson =
            TestSettingsEventFactory.GetExpectedResultSettings(serviceName, environmentName);
        var expectedResultProjection = new SettingsProjection(
            expectedResultSettingsJson,
            settingsEvents.Last().TimeStamp,
            settingsEvents.Last().Version);
        var settingsCreateEvent = settingsEvents[0];
        var settingsAggregate = new SettingsAggregate(settingsCreateEvent.Metadata);

        // Act
        foreach (var settingsEvent in settingsEvents)
        {
            var applyResult = settingsAggregate.TryApplyEvent(settingsEvent, out var error);

            if(!applyResult)
                throw new InvalidOperationException($"Failed to apply event: {error!.Value.Description}");
        }

        // Assert
        await Assert.That(settingsAggregate.Metadata).IsEqualTo(settingsCreateEvent.Metadata);
        await Assert.That(settingsAggregate.CurrentProjection).IsEqualTo(expectedResultProjection);
    }

    [Test]
    public async Task ApplyEvent_ValidationFails_WhenEventMetadataDoesNotMatch()
    {
        // Arrange
        var settingsCreateEvent = TestSettingsEventFactory.CreateSettingsCreateEvent();
        var settingsAggregate = new SettingsAggregate(
            new SettingsMetadata("wrongService", "wrongEnvironment"));

        // Act
        var applyResult = settingsAggregate.TryApplyEvent(settingsCreateEvent, out var error);

        // Assert
        await Assert.That(applyResult).IsFalse();
        await Assert.That(error!.Value.Type).IsEqualTo(ErrorType.Validation);
    }

    [Test]
    public async Task ApplyEvent_WhenApplyingCreateEvent_ValidationFails_IfEventVersionIsNotZero()
    {
        // Arrange
        var settingsCreateEvent = TestSettingsEventFactory.CreateSettingsCreateEvent() with { Version = 10 };
        var settingsAggregate = new SettingsAggregate(settingsCreateEvent.Metadata);

        // Act
        var applyResult = settingsAggregate.TryApplyEvent(settingsCreateEvent, out var error);

        // Assert
        await Assert.That(applyResult).IsFalse();
        await Assert.That(error!.Value.Type).IsEqualTo(ErrorType.Validation);
    }

    [Test]
    public async Task ApplyEvent_WhenApplyingCreateEvent_ValidationFails_IfCurrentProjectionIsNotNull()
    {
        // Arrange
        var settingsCreateEvent = TestSettingsEventFactory.CreateSettingsCreateEvent();
        var settingsAggregate = new SettingsAggregate(
            settingsCreateEvent.Metadata,
            new SettingsProjection("not-json", 0, 0));

        // Act
        var applyResult = settingsAggregate.TryApplyEvent(settingsCreateEvent, out var error);

        //Assert
        await Assert.That(applyResult).IsFalse();
        await Assert.That(error!.Value.Type).IsEqualTo(ErrorType.Validation);
    }

    [Test]
    public async Task ApplyEvent_WhenApplyingUpdateEvent_ValidationFails_IfCurrentProjectionIsNull()
    {
        // Arrange
        var settingsUpdateEvent = TestSettingsEventFactory.CreateSettingsUpdateEvent();
        var settingsAggregate = new SettingsAggregate(settingsUpdateEvent.Metadata);

        // Act
        var applyResult = settingsAggregate.TryApplyEvent(settingsUpdateEvent, out var error);

        //Assert
        await Assert.That(applyResult).IsFalse();
        await Assert.That(error!.Value.Type).IsEqualTo(ErrorType.Validation);
    }

    [Test]
    public async Task ApplyEvent_WhenApplyingUpdateEvent_ValidationFails_IfEventVersionIsNotGreaterThanCurrentProjection()
    {
        // Arrange
        var settingsEvents = TestSettingsEventFactory
            .CreateSettingsEvents()
            .Where(se => se.Metadata is {ServiceName: "service1", EnvironmentName:"environment1" })
            .ToArray();
        var settingsUpdateEvent = settingsEvents[1];
        var settingsCreateEvent = settingsEvents[0];
        var initialProjection = new SettingsProjection(
            settingsCreateEvent.JsonData,
            settingsCreateEvent.TimeStamp,
            settingsCreateEvent.Version);
        var settingsAggregate = new SettingsAggregate(settingsUpdateEvent.Metadata, initialProjection);

        // Act
        var applyResult = settingsAggregate.TryApplyEvent
            (settingsUpdateEvent with { Version = 100 }, out var error);

        //Assert
        await Assert.That(applyResult).IsFalse();
        await Assert.That(error!.Value.Type).IsEqualTo(ErrorType.Validation);
    }

    [Test]
    public async Task ApplyEvent_WhenApplyingUpdateEvent_ValidationFails_IfEventTimestampNotGreaterThanCurrentProjectionTimestamp()
    {
        // Arrange
        var settingsEvents = TestSettingsEventFactory
            .CreateSettingsEvents()
            .Where(se => se.Metadata is {ServiceName: "service1", EnvironmentName:"environment1" })
            .ToArray();
        var settingsUpdateEvent = settingsEvents[1];
        var settingsCreateEvent = settingsEvents[0];
        var initialProjection = new SettingsProjection(
            settingsCreateEvent.JsonData,
            settingsCreateEvent.TimeStamp,
            settingsCreateEvent.Version);
        var settingsAggregate = new SettingsAggregate(settingsUpdateEvent.Metadata, initialProjection);

        // Act
        var applyResult = settingsAggregate.TryApplyEvent
            (settingsUpdateEvent with { TimeStamp = 1 }, out var error);

        //Assert
        await Assert.That(applyResult).IsFalse();
        await Assert.That(error!.Value.Type).IsEqualTo(ErrorType.Validation);
    }

    [Test]
    public async Task ApplyEvent_WhenApplyingUpdateEvent_ValidationFails_IfJsonPatchBodyIsNotValid()
    {
        // Arrange
        var settingsEvents = TestSettingsEventFactory
            .CreateSettingsEvents()
            .Where(se => se.Metadata is {ServiceName: "service1", EnvironmentName:"environment1" })
            .ToArray();
        var settingsUpdateEvent = settingsEvents[1] with { JsonData = "not-valid-json" };
        var settingsCreateEvent = settingsEvents[0];
        var initialProjection = new SettingsProjection(
            settingsCreateEvent.JsonData,
            settingsCreateEvent.TimeStamp,
            settingsCreateEvent.Version);
        var settingsAggregate = new SettingsAggregate(settingsUpdateEvent.Metadata, initialProjection);

        // Act
        var applyResult = settingsAggregate.TryApplyEvent
            (settingsUpdateEvent with { TimeStamp = 1 }, out var error);

        //Assert
        await Assert.That(applyResult).IsFalse();
        await Assert.That(error!.Value.Type).IsEqualTo(ErrorType.Validation);
    }

    [Test]
    public async Task ApplyEvent_WhenApplyingUnsupportedEventType_ValidationFails()
    {
        //Arrange
        var settingsEvent = TestSettingsEventFactory.CreateSettingsCreateEvent() with
        {
            EventType = (SettingsEventType) 100
        };
        var settingsAggregate = new SettingsAggregate(settingsEvent.Metadata);

        // Act
        var applyResult = settingsAggregate.TryApplyEvent(settingsEvent, out var error);

        //Assert
        await Assert.That(applyResult).IsFalse();
        await Assert.That(error!.Value.Type).IsEqualTo(ErrorType.Validation);
    }
}
