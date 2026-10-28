using BeyondCrud.Domain.Commands;
using BeyondCrud.Domain.Events;

namespace BeyondCrud.Domain;

public class Flight
{
    private readonly List<IDomainEvent> _events = [];
    private readonly List<Guid> _passengerIds = [];
    private readonly Dictionary<string, Guid> _seats = [];
    private readonly HashSet<Guid> _checkedInPassengerIds = [];

    public Guid Id { get; private set; }
    public string Destination { get; private set; } = "";
    public int Capacity { get; private set; }
    public RegistrationStatus Registration { get; private set; }
    public IReadOnlyList<Guid> PassengerIds => _passengerIds;
    public IReadOnlyDictionary<string, Guid> Seats => _seats;
    public IReadOnlyCollection<Guid> CheckedInPassengerIds => _checkedInPassengerIds;

    public IReadOnlyList<IDomainEvent> GetUncommittedEvents()
        => _events;

    public void ClearUncommittedEvents()
        => _events.Clear();

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
    {
        if (Registration != RegistrationStatus.Open)
            throw new DomainException("Registration is not open.");

        if (_passengerIds.Contains(command.PassengerId))
            throw new DomainException("Passenger is already registered.");

        if (_passengerIds.Count >= Capacity)
            throw new DomainException("Flight is full.");

        Raise(new PassengerRegistered(command.FlightId, command.PassengerId, command.Name));
    }

    public void AssignSeat(AssignSeat command)
    {
        if (!_passengerIds.Contains(command.PassengerId))
            throw new DomainException("Passenger is not registered.");

        if (_seats.ContainsKey(command.Seat))
            throw new DomainException("Seat is already taken.");

        Raise(new SeatAssigned(command.FlightId, command.PassengerId, command.Seat));
    }

    public void CheckIn(CheckInPassenger command)
    {
        if (Registration != RegistrationStatus.Closed)
            throw new DomainException("Check-in opens once registration is closed.");

        if (!_passengerIds.Contains(command.PassengerId))
            throw new DomainException("Passenger is not registered.");

        if (_checkedInPassengerIds.Contains(command.PassengerId))
            throw new DomainException("Passenger is already checked in.");

        Raise(new PassengerCheckedIn(command.FlightId, command.PassengerId));
    }

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

            case RegistrationOpened:
                Registration = RegistrationStatus.Open;
                break;

            case PassengerRegistered e:
                _passengerIds.Add(e.PassengerId);
                break;

            case RegistrationClosed:
                Registration = RegistrationStatus.Closed;
                break;

            case SeatAssigned e:
                _seats[e.Seat] = e.PassengerId;
                break;

            case PassengerCheckedIn e:
                _checkedInPassengerIds.Add(e.PassengerId);
                break;
        }
    }
}
