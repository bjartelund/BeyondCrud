using BeyondCrud.Domain.Events;

namespace BeyondCrud.Domain.Tests;

public class PassengerTests
{
    [Test]
    public async Task Register_emits_passenger_registered_event()
    {
        // Arrange
        var passengerId = Guid.NewGuid();

        var passenger = new Passenger();

        // Act
        passenger.Register(
            passengerId,
            "Bjarte");

        // Assert
        await Assert.That(passenger.GetUncommittedEvents())
            .Count().IsEqualTo(1);

        var @event =
            passenger.GetUncommittedEvents().Single();

        await Assert.That(@event)
            .IsTypeOf<PassengerRegistered>();

        var registered =
            (PassengerRegistered)@event;

        await Assert.That(registered.PassengerId)
            .IsEqualTo(passengerId);

        await Assert.That(registered.Name)
            .IsEqualTo("Bjarte");
    }
}