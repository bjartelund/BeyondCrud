using BeyondCrud.Domain.Commands;
using BeyondCrud.Domain.Events;

namespace BeyondCrud.Domain.Tests;

public class FlightTests
{
    private static readonly Guid FlightId = Guid.NewGuid();

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
        var flight = new Flight();
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
}
