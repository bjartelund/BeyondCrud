using BeyondCrud.Domain.Commands;
using BeyondCrud.Domain.Events;

namespace BeyondCrud.Domain.Tests;

public class FlightTests
{
    private static readonly Guid FlightId = Guid.NewGuid();

    private static Flight OpenFlight(int capacity = 2, params IDomainEvent[] more)
        => Flight.Rehydrate(
            [
                new FlightCreated(FlightId, "Oslo", capacity),
                new RegistrationOpened(FlightId),
                ..more
            ]);

    [Test]
    public async Task Create_emits_flight_created_event()
    {
        // Arrange
        var flight = new Flight();

        // Act
        flight.Create(new CreateFlight(FlightId, "Oslo", 2));

        // Assert
        var @event = flight.GetUncommittedEvents().Single();

        await Assert.That(@event)
            .IsEqualTo(new FlightCreated(FlightId, "Oslo", 2));
    }

    [Test]
    public async Task OpenRegistration_emits_registration_opened_event()
    {
        // Arrange
        var flight = new Flight();

        // Act
        flight.OpenRegistration(new OpenRegistration(FlightId));

        // Assert
        await Assert.That(flight.GetUncommittedEvents())
            .Contains(new RegistrationOpened(FlightId));
    }

    [Test]
    public async Task RegisterPassenger_emits_passenger_registered_event()
    {
        // Arrange
        var flight = OpenFlight();
        var passengerId = Guid.NewGuid();

        // Act
        flight.RegisterPassenger(new RegisterPassenger(FlightId, passengerId, "Bjarte"));

        // Assert
        var @event = flight.GetUncommittedEvents().Single();

        await Assert.That(@event)
            .IsEqualTo(new PassengerRegistered(FlightId, passengerId, "Bjarte"));
    }

    [Test]
    public async Task CloseRegistration_emits_registration_closed_event()
    {
        // Arrange
        var flight = new Flight();

        // Act
        flight.CloseRegistration(new CloseRegistration(FlightId));

        // Assert
        await Assert.That(flight.GetUncommittedEvents())
            .Contains(new RegistrationClosed(FlightId));
    }

    [Test]
    public async Task Rehydrate_restores_registration_status()
    {
        // Arrange
        var history = new IDomainEvent[]
        {
            new FlightCreated(FlightId, "Oslo", 2),
            new RegistrationOpened(FlightId),
            new RegistrationClosed(FlightId)
        };

        // Act
        var flight = Flight.Rehydrate(history);

        // Assert
        await Assert.That(flight.Registration)
            .IsEqualTo(RegistrationStatus.Closed);
    }

    [Test]
    public async Task Rehydrate_restores_registered_passengers()
    {
        // Arrange
        var history = new IDomainEvent[]
        {
            new FlightCreated(FlightId, "Oslo", 2),
            new RegistrationOpened(FlightId),
            new PassengerRegistered(FlightId, Guid.NewGuid(), "Ada"),
            new PassengerRegistered(FlightId, Guid.NewGuid(), "Grace")
        };

        // Act
        var flight = Flight.Rehydrate(history);

        // Assert
        await Assert.That(flight.PassengerIds)
            .Count().IsEqualTo(2);
    }

    [Test]
    public async Task Rehydrate_does_not_produce_uncommitted_events()
    {
        // Arrange
        var history = new IDomainEvent[] { new FlightCreated(FlightId, "Oslo", 2) };

        // Act
        var flight = Flight.Rehydrate(history);
        flight.OpenRegistration(new OpenRegistration(FlightId));

        // Assert
        await Assert.That(flight.GetUncommittedEvents().Single())
            .IsEqualTo(new RegistrationOpened(FlightId));
    }

    [Test]
    public async Task RegisterPassenger_is_rejected_when_registration_is_closed()
    {
        // Arrange
        var flight = OpenFlight(2, new RegistrationClosed(FlightId));

        // Act / Assert
        await Assert.That(() => flight.RegisterPassenger(new RegisterPassenger(FlightId, Guid.NewGuid(), "Ada")))
            .Throws<DomainException>();
    }

    [Test]
    public async Task RegisterPassenger_is_rejected_when_registration_has_not_opened()
    {
        // Arrange
        var flight = Flight.Rehydrate([new FlightCreated(FlightId, "Oslo", 2)]);

        // Act / Assert
        await Assert.That(() => flight.RegisterPassenger(new RegisterPassenger(FlightId, Guid.NewGuid(), "Ada")))
            .Throws<DomainException>();
    }

    [Test]
    public async Task RegisterPassenger_is_rejected_for_duplicate_passenger()
    {
        // Arrange
        var passengerId = Guid.NewGuid();
        var flight = OpenFlight(2, new PassengerRegistered(FlightId, passengerId, "Ada"));

        // Act / Assert
        await Assert.That(() => flight.RegisterPassenger(new RegisterPassenger(FlightId, passengerId, "Ada")))
            .Throws<DomainException>();
    }

    [Test]
    public async Task RegisterPassenger_is_rejected_when_flight_is_full()
    {
        // Arrange
        var flight = OpenFlight(1, new PassengerRegistered(FlightId, Guid.NewGuid(), "Ada"));

        // Act / Assert
        await Assert.That(() => flight.RegisterPassenger(new RegisterPassenger(FlightId, Guid.NewGuid(), "Grace")))
            .Throws<DomainException>();
    }

    [Test]
    public async Task AssignSeat_emits_seat_assigned_event()
    {
        // Arrange
        var passengerId = Guid.NewGuid();
        var flight = OpenFlight(2, new PassengerRegistered(FlightId, passengerId, "Ada"));

        // Act
        flight.AssignSeat(new AssignSeat(FlightId, passengerId, "1A"));

        // Assert
        await Assert.That(flight.GetUncommittedEvents().Single())
            .IsEqualTo(new SeatAssigned(FlightId, passengerId, "1A"));
    }

    [Test]
    public async Task AssignSeat_is_rejected_for_unregistered_passenger()
    {
        // Arrange
        var flight = OpenFlight();

        // Act / Assert
        await Assert.That(() => flight.AssignSeat(new AssignSeat(FlightId, Guid.NewGuid(), "1A")))
            .Throws<DomainException>();
    }

    [Test]
    public async Task AssignSeat_is_rejected_when_seat_is_taken()
    {
        // Arrange
        var ada = Guid.NewGuid();
        var grace = Guid.NewGuid();
        var flight = OpenFlight(
            2,
            new PassengerRegistered(FlightId, ada, "Ada"),
            new PassengerRegistered(FlightId, grace, "Grace"),
            new SeatAssigned(FlightId, ada, "1A"));

        // Act / Assert
        await Assert.That(() => flight.AssignSeat(new AssignSeat(FlightId, grace, "1A")))
            .Throws<DomainException>();
    }
}
