using BeyondCrud.Domain.Commands;

namespace BeyondCrud.Domain;

public class Passenger
{
    private readonly List<IDomainEvent> _events = [];

    public IReadOnlyList<IDomainEvent> GetUncommittedEvents()
        => _events;

    public void Register(RegisterPassenger command)
    {
        // TODO: record that the passenger was registered by adding a PassengerRegistered event
    }
}
