using BeyondCrud.Infrastructure;
using EventStore.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BeyondCrud.Projections;

/// <summary>
/// Catches up on every flight stream, then follows new events as they are appended.
/// There is no checkpoint, so each start replays history into an empty read model.
/// </summary>
public sealed class FlightProjectionService(
    EventStoreClient client,
    FlightListReadModel readModel,
    FlightListProjection projection,
    ILogger<FlightProjectionService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Replaying from the start, so forget what an earlier attempt applied.
                readModel.Clear();

                await using var subscription = client.SubscribeToAll(
                    FromAll.Start,
                    filterOptions: new SubscriptionFilterOptions(StreamFilter.Prefix("flight-")),
                    cancellationToken: stoppingToken);

                await foreach (var message in subscription.Messages.WithCancellation(stoppingToken))
                {
                    if (message is not StreamMessage.Event(var resolved))
                        continue;

                    projection.Apply(EventSerializer.Deserialize(resolved.Event.EventType, resolved.Event.Data));
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Flight projection subscription dropped, restarting.");
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }
}