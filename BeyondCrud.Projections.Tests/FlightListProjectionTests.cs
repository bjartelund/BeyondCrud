using BeyondCrud.Domain;
using BeyondCrud.Domain.Events;

namespace BeyondCrud.Projections.Tests;

public class FlightListProjectionTests
{
    private static readonly Guid FlightId = Guid.NewGuid();

    private static (FlightListReadModel ReadModel, FlightListProjection Projection) Setup()
    {
        var readModel = new FlightListReadModel();
        return (readModel, new FlightListProjection(readModel));
    }

    private static void Apply(FlightListProjection projection, params IDomainEvent[] events)
    {
        foreach (var @event in events)
            projection.Apply(@event);
    }

    [Test]
    public async Task Created_flight_appears_in_the_list()
    {
        // Arrange
        var (readModel, projection) = Setup();

        // Act
        Apply(projection, new FlightCreated(FlightId, "Oslo", 2));

        // Assert
        var flight = readModel.Get(FlightId)!;

        await Assert.That(flight.Destination).IsEqualTo("Oslo");
        await Assert.That(flight.Registration).IsEqualTo(RegistrationStatus.NotOpen);
    }

    [Test]
    public async Task Opened_registration_is_reflected()
    {
        // Arrange
        var (readModel, projection) = Setup();

        // Act
        Apply(projection, new FlightCreated(FlightId, "Oslo", 2), new RegistrationOpened(FlightId));

        // Assert
        await Assert.That(readModel.Get(FlightId)!.Registration).IsEqualTo(RegistrationStatus.Open);
    }

    [Test]
    public async Task Registered_passengers_are_listed_by_name()
    {
        // Arrange
        var (readModel, projection) = Setup();

        // Act
        Apply(
            projection,
            new FlightCreated(FlightId, "Oslo", 2),
            new RegistrationOpened(FlightId),
            new PassengerRegistered(FlightId, Guid.NewGuid(), "Ada"),
            new PassengerRegistered(FlightId, Guid.NewGuid(), "Grace"));

        // Assert
        await Assert.That(readModel.Get(FlightId)!.Passengers).IsEquivalentTo(new[] { "Ada", "Grace" });
    }

    [Test]
    public async Task Closed_registration_is_reflected()
    {
        // Arrange
        var (readModel, projection) = Setup();

        // Act
        Apply(
            projection,
            new FlightCreated(FlightId, "Oslo", 2),
            new RegistrationOpened(FlightId),
            new RegistrationClosed(FlightId));

        // Assert
        await Assert.That(readModel.Get(FlightId)!.Registration).IsEqualTo(RegistrationStatus.Closed);
    }

    [Test]
    public async Task Events_for_an_unknown_flight_are_ignored()
    {
        // Arrange
        var (readModel, projection) = Setup();

        // Act
        Apply(projection, new RegistrationOpened(Guid.NewGuid()));

        // Assert
        await Assert.That(readModel.List()).IsEmpty();
    }
}