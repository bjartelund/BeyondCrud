using BeyondCrud.Domain.Commands;
using BeyondCrud.Domain.Events;

namespace BeyondCrud.Domain;

public class Passenger
{
    private readonly List<IDomainEvent> _events = [];

    public IReadOnlyList<IDomainEvent> GetUncommittedEvents()
        => _events;

    public void Register(RegisterPassenger command)
    {
        _events.Add(new PassengerRegistered(command.PassengerId, command.Name));
    }
}
