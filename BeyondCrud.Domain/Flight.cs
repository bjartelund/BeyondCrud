using BeyondCrud.Domain.Commands;
using BeyondCrud.Domain.Events;

namespace BeyondCrud.Domain;

public class Flight
{
    private readonly List<IDomainEvent> _events = [];

    public IReadOnlyList<IDomainEvent> GetUncommittedEvents()
        => _events;

    public void Create(CreateFlight command)
    {
        _events.Add(new FlightCreated(command.FlightId, command.Destination, command.Capacity));
    }

    public void OpenRegistration(OpenRegistration command)
    {
        // TODO: record that registration was opened
    }

    public void RegisterPassenger(RegisterPassenger command)
    {
        _events.Add(new PassengerRegistered(command.FlightId, command.PassengerId, command.Name));
    }

    public void CloseRegistration(CloseRegistration command)
    {
        // TODO: record that registration was closed
    }
}
