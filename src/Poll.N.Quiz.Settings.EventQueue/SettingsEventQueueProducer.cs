using MassTransit;
using Poll.N.Quiz.Settings.Domain.ValueObjects;

namespace Poll.N.Quiz.Settings.EventQueue;

public class SettingsEventQueueProducer(ITopicProducer<SettingsEvent> topicProducer)
{
    public const string TopicName = "settings-event-topic";

    public Task SendAsync(SettingsEvent settingsEvent, CancellationToken cancellationToken = default) =>
        topicProducer.Produce(settingsEvent, cancellationToken);
}
