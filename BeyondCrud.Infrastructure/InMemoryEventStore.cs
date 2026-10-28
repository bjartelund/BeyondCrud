using System.Collections.Concurrent;
using BeyondCrud.Domain;

namespace BeyondCrud.Infrastructure;

/// <summary>
/// For tests: same contract as the real store, including optimistic concurrency, without a database.
/// </summary>
public sealed class InMemoryEventStore : IEventStore
{
    private readonly ConcurrentDictionary<string, List<IDomainEvent>> _streams = new();

    public Task<IReadOnlyList<IDomainEvent>> ReadStream(string streamName, CancellationToken cancellationToken = default)
    {
        if (!_streams.TryGetValue(streamName, out var stream))
            return Task.FromResult<IReadOnlyList<IDomainEvent>>([]);

        lock (stream)
            return Task.FromResult<IReadOnlyList<IDomainEvent>>(stream.ToArray());
    }

    public Task Append(
        string streamName,
        long expectedVersion,
        IReadOnlyList<IDomainEvent> events,
        CancellationToken cancellationToken = default)
    {
        var stream = _streams.GetOrAdd(streamName, _ => []);

        lock (stream)
        {
            if (stream.Count - 1 != expectedVersion)
                throw new ConcurrencyException(streamName, expectedVersion);

            stream.AddRange(events);
        }

        return Task.CompletedTask;
    }
}