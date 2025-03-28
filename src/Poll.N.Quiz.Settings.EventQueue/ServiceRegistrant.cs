using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Poll.N.Quiz.Settings.Domain.ValueObjects;

namespace Poll.N.Quiz.Settings.EventQueue;

public static class ServiceRegistrant
{

    public static IServiceCollection AddSettingsEventQueueProducer(
        this IServiceCollection services,
        string connectionString,
        string topicName) =>
        services
            .AddMassTransit(x =>
            {
                x.UsingInMemory();
                x.AddRider(rider =>
                {
                    rider.AddProducer<SettingsEvent>(topicName);

                    rider.UsingKafka((context, k) =>
                    {
                        k.Host(connectionString);
                    });
                });
            })
            .AddSingleton<SettingsEventQueueProducer>();

    public static IServiceCollection AddSettingsEventQueueConsumer<TConsumer>(
        this IServiceCollection services,
        string connectionString,
        string topicName)
        where TConsumer : class, IConsumer<SettingsEvent> =>
        services
            .AddMassTransit(x =>
            {
                x.UsingInMemory();
                x.AddRider(rider =>
                {
                    rider.AddConsumer<TConsumer>();

                    rider.UsingKafka((context, k) =>
                    {
                        k.Host(connectionString);

                        k.TopicEndpoint<SettingsEvent>(topicName, typeof(SettingsEvent).FullName, e =>
                        {
                            e.ConfigureConsumer<TConsumer>(context);
                        });
                    });
                });
            })
            .AddSingleton<SettingsEventQueueProducer>();

    public static IServiceCollection AddSettingsEventQueueProducerAndConsumer<TConsumer>(
        this IServiceCollection services,
        string connectionString,
        string topicName)
        where TConsumer : class, IConsumer<SettingsEvent> =>
        services
            .AddMassTransit(x =>
            {
                x.UsingInMemory();
                x.AddRider(rider =>
                {
                    rider.AddConsumer<TConsumer>(); //user provides implementation of consumer
                    rider.AddProducer<SettingsEvent>(topicName); //mass transit provides implementation of producer

                    rider.UsingKafka((context, k) =>
                    {
                        k.Host(connectionString);

                        k.TopicEndpoint<SettingsEvent>(topicName, typeof(SettingsEvent).FullName, e =>
                        {
                            e.ConfigureConsumer<TConsumer>(context);
                        });
                    });
                });
            })
            .AddSingleton<SettingsEventQueueProducer>();
}
