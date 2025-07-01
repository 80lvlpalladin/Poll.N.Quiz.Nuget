using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Poll.N.Quiz.Settings.Domain.ValueObjects;

namespace Poll.N.Quiz.Settings.EventQueue;

public static class ServiceRegistrant
{
    public static IServiceCollection AddSettingsEventQueueProducerAndConsumer<TConsumer>
        (this IServiceCollection services, string settingsEventQueueConnectionString)
        where TConsumer : class, IConsumer<SettingsEvent> =>
        services
            .AddMassTransit(x =>
            {
                x.UsingInMemory();
                x.AddRider(rider =>
                {
                    rider.AddConsumer<TConsumer>(); //user provides implementation of consumer
                    rider.AddProducer<SettingsEvent>(SettingsEventQueueProducer.TopicName); //mass transit provides implementation of producer

                    rider.UsingKafka((context, k) =>
                    {
                        k.Host(settingsEventQueueConnectionString);

                        k.TopicEndpoint<SettingsEvent>(SettingsEventQueueProducer.TopicName, typeof(SettingsEvent).FullName, e =>
                        {
                            e.ConfigureConsumer<TConsumer>(context);
                            e.CreateIfMissing(x => {
                                x.NumPartitions = 1;
                                x.ReplicationFactor = 1;
                            });
                        });
                    });
                });
            })
            .AddSingleton<SettingsEventQueueProducer>();
}
