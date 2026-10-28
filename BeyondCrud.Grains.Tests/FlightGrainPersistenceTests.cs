using BeyondCrud.Domain.Commands;
using BeyondCrud.Domain.Events;
using BeyondCrud.Infrastructure;

namespace BeyondCrud.Grains.Tests;

// Deactivating every grain would pull the rug from tests running alongside, so these run on their own.
[NotInParallel]
[ClassDataSource<ClusterFixture>(Shared = SharedType.PerTestSession)]
public class FlightGrainPersistenceTests(ClusterFixture fixture)
{
    private async Task DeactivateAll()
        => await fixture.Cluster.Client.GetGrain<IManagementGrain>(0).ForceActivationCollection(TimeSpan.Zero);

    [Test]
    public async Task Commands_are_appended_to_the_flights_stream()
    {
        // Arrange
        var flightId = Guid.NewGuid();
        var grain = fixture.Cluster.GrainFactory.GetGrain<IFlightGrain>(flightId);
        var passengerId = Guid.NewGuid();

        // Act
        await grain.Create(new CreateFlight(flightId, "Oslo", 2));
        await grain.OpenRegistration(new OpenRegistration(flightId));
        await grain.RegisterPassenger(new RegisterPassenger(flightId, passengerId, "Ada"));

        // Assert
        var stored = await ClusterFixture.EventStore.ReadStream($"flight-{flightId}");

        Domain.IDomainEvent[] expected =
        [
            new FlightCreated(flightId, "Oslo", 2),
            new RegistrationOpened(flightId),
            new PassengerRegistered(flightId, passengerId, "Ada")
        ];

        await Assert.That(stored).IsEquivalentTo(expected);
    }

    [Test]
    public async Task State_survives_the_grain_being_deactivated()
    {
        // Arrange
        var flightId = Guid.NewGuid();
        var grain = fixture.Cluster.GrainFactory.GetGrain<IFlightGrain>(flightId);
        var passengerId = Guid.NewGuid();

        await grain.Create(new CreateFlight(flightId, "Oslo", 2));
        await grain.OpenRegistration(new OpenRegistration(flightId));
        await grain.RegisterPassenger(new RegisterPassenger(flightId, passengerId, "Ada"));

        // Act
        await DeactivateAll();

        // Assert
        var summary = await grain.GetSummary();

        await Assert.That(summary.Destination).IsEqualTo("Oslo");
        await Assert.That(summary.PassengerIds).Contains(passengerId);
    }

    [Test]
    public async Task Rehydrated_grain_keeps_appending_at_the_right_version()
    {
        // Arrange
        var flightId = Guid.NewGuid();
        var grain = fixture.Cluster.GrainFactory.GetGrain<IFlightGrain>(flightId);

        await grain.Create(new CreateFlight(flightId, "Oslo", 2));
        await DeactivateAll();

        // Act
        await grain.OpenRegistration(new OpenRegistration(flightId));

        // Assert
        var stored = await ClusterFixture.EventStore.ReadStream($"flight-{flightId}");

        await Assert.That(stored).Count().IsEqualTo(2);
    }

    [Test]
    public async Task Event_serializer_round_trips_every_event_type()
    {
        // Arrange
        var flightId = Guid.NewGuid();
        var passengerId = Guid.NewGuid();

        Domain.IDomainEvent[] events =
        [
            new FlightCreated(flightId, "Oslo", 2),
            new RegistrationOpened(flightId),
            new PassengerRegistered(flightId, passengerId, "Ada"),
            new SeatAssigned(flightId, passengerId, "1A"),
            new RegistrationClosed(flightId)
        ];

        // Act
        var roundTripped = events
            .Select(EventSerializer.Serialize)
            .Select(stored => EventSerializer.Deserialize(stored.EventType, stored.Data))
            .ToArray();

        // Assert
        await Assert.That(roundTripped).IsEquivalentTo(events);
    }
}