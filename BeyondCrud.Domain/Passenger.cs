using BeyondCrud.Domain.Events;

namespace BeyondCrud.Domain;

public class Passenger
{
    private readonly List<object> _events = [];

    public IReadOnlyList<object> GetUncommittedEvents()
        => _events;

    public void Register(
        Guid passengerId,
        string name)
    {
        var @event =
            new PassengerRegistered(
                passengerId,
                name);

        _events.Add(@event);
    }
}