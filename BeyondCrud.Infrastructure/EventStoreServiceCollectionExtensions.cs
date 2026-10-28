using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;

namespace BeyondCrud.Infrastructure;

public static class EventStoreServiceCollectionExtensions
{
    public static IServiceCollection AddEventStoreDb(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton(new EventStoreClient(EventStoreClientSettings.Create(connectionString)));
        services.AddSingleton<IEventStore, EventStoreDbEventStore>();

        return services;
    }
}