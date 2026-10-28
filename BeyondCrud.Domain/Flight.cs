using BeyondCrud.Domain.Commands;
using BeyondCrud.Domain.Events;

namespace BeyondCrud.Domain;

public class Flight
{
    private readonly List<IDomainEvent> _events = [];
    private readonly List<Guid> _passengerIds = [];

    public Guid Id { get; private set; }
    public string Destination { get; private set; } = "";
    public int Capacity { get; private set; }
    public RegistrationStatus Registration { get; private set; }
    public IReadOnlyList<Guid> PassengerIds => _passengerIds;

    public IReadOnlyList<IDomainEvent> GetUncommittedEvents()
        => _events;

    public static Flight Rehydrate(IEnumerable<IDomainEvent> history)
    {
        var flight = new Flight();

        foreach (var @event in history)
            flight.Apply(@event);

        return flight;
    }

    public void Create(CreateFlight command)
        => Raise(new FlightCreated(command.FlightId, command.Destination, command.Capacity));

    public void OpenRegistration(OpenRegistration command)
        => Raise(new RegistrationOpened(command.FlightId));

    public void RegisterPassenger(RegisterPassenger command)
        => Raise(new PassengerRegistered(command.FlightId, command.PassengerId, command.Name));

    public void CloseRegistration(CloseRegistration command)
        => Raise(new RegistrationClosed(command.FlightId));

    private void Raise(IDomainEvent @event)
    {
        Apply(@event);
        _events.Add(@event);
    }

    private void Apply(IDomainEvent @event)
    {
        switch (@event)
        {
            case FlightCreated e:
                Id = e.FlightId;
                Destination = e.Destination;
                Capacity = e.Capacity;
                break;

            // TODO: fold RegistrationOpened, PassengerRegistered and RegistrationClosed into state
        }
    }
}
