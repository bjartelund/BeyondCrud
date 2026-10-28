using System.Collections.Concurrent;

namespace BeyondCrud.Projections;

/// <summary>
/// The read side: a denormalised view shaped for queries, never used to make decisions.
/// Kept in memory and rebuilt from the event streams on startup.
/// </summary>
public sealed class FlightListReadModel
{
    private readonly ConcurrentDictionary<Guid, FlightListItem> _flights = new();

    public IReadOnlyList<FlightListItem> List()
        => _flights.Values.OrderBy(flight => flight.Destination).ThenBy(flight => flight.FlightId).ToArray();

    public FlightListItem? Get(Guid flightId)
        => _flights.GetValueOrDefault(flightId);

    public void Set(FlightListItem flight)
        => _flights[flight.FlightId] = flight;

    public void Update(Guid flightId, Func<FlightListItem, FlightListItem> change)
    {
        if (_flights.TryGetValue(flightId, out var current))
            _flights[flightId] = change(current);
    }

    public void Clear()
        => _flights.Clear();
}