using Microsoft.Extensions.DependencyInjection;

namespace BeyondCrud.Projections;

public static class ProjectionServiceCollectionExtensions
{
    /// <summary>Needs an <see cref="EventStore.Client.EventStoreClient"/>, see <c>AddEventStoreDb</c>.</summary>
    public static IServiceCollection AddFlightProjections(this IServiceCollection services)
    {
        services.AddSingleton<FlightListReadModel>();
        services.AddSingleton<FlightListProjection>();
        services.AddHostedService<FlightProjectionService>();

        return services;
    }
}