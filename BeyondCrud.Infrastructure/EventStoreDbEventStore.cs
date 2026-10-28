using BeyondCrud.Domain;
using EventStore.Client;

namespace BeyondCrud.Infrastructure;

public sealed class EventStoreDbEventStore(EventStoreClient client) : IEventStore
{
    public async Task<IReadOnlyList<IDomainEvent>> ReadStream(
        string streamName,
        CancellationToken cancellationToken = default)
    {
        var result = client.ReadStreamAsync(
            Direction.Forwards,
            streamName,
            StreamPosition.Start,
            cancellationToken: cancellationToken);

        if (await result.ReadState == ReadState.StreamNotFound)
            return [];

        var events = new List<IDomainEvent>();

        await foreach (var resolved in result)
            events.Add(EventSerializer.Deserialize(resolved.Event.EventType, resolved.Event.Data));

        return events;
    }

    public async Task Append(
        string streamName,
        long expectedVersion,
        IReadOnlyList<IDomainEvent> events,
        CancellationToken cancellationToken = default)
    {
        var eventData = events.Select(@event =>
        {
            var (eventType, data) = EventSerializer.Serialize(@event);
            return new EventData(Uuid.NewUuid(), eventType, data);
        });

        try
        {
            if (expectedVersion < 0)
                await client.AppendToStreamAsync(streamName, StreamState.NoStream, eventData, cancellationToken: cancellationToken);
            else
                await client.AppendToStreamAsync(streamName, StreamRevision.FromInt64(expectedVersion), eventData, cancellationToken: cancellationToken);
        }
        catch (WrongExpectedVersionException exception)
        {
            throw new ConcurrencyException(streamName, expectedVersion, exception);
        }
    }
}