using BeyondCrud.Domain;

namespace BeyondCrud.Infrastructure;

/// <summary>
/// One stream of events per aggregate. Versions are zero-based event numbers; -1 means the stream does not exist yet.
/// </summary>
public interface IEventStore
{
    Task<IReadOnlyList<IDomainEvent>> ReadStream(string streamName, CancellationToken cancellationToken = default);

    /// <exception cref="ConcurrencyException">The stream is not at <paramref name="expectedVersion"/>.</exception>
    Task Append(
        string streamName,
        long expectedVersion,
        IReadOnlyList<IDomainEvent> events,
        CancellationToken cancellationToken = default);
}