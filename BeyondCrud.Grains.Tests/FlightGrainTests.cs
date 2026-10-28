using BeyondCrud.Domain;
using BeyondCrud.Domain.Commands;

namespace BeyondCrud.Grains.Tests;

[ClassDataSource<ClusterFixture>(Shared = SharedType.PerTestSession)]
public class FlightGrainTests(ClusterFixture fixture)
{
    private IFlightGrain NewFlight(out Guid flightId)
    {
        flightId = Guid.NewGuid();
        return fixture.Cluster.GrainFactory.GetGrain<IFlightGrain>(flightId);
    }

    [Test]
    public async Task Created_flight_is_visible_in_summary()
    {
        // Arrange
        var grain = NewFlight(out var flightId);

        // Act
        await grain.Create(new CreateFlight(flightId, "Oslo", 2));
        await grain.OpenRegistration(new OpenRegistration(flightId));

        // Assert
        var summary = await grain.GetSummary();

        await Assert.That(summary.Destination).IsEqualTo("Oslo");
        await Assert.That(summary.Registration).IsEqualTo(RegistrationStatus.Open);
    }

    [Test]
    public async Task Registered_passenger_is_visible_in_summary()
    {
        // Arrange
        var grain = NewFlight(out var flightId);
        var passengerId = Guid.NewGuid();

        await grain.Create(new CreateFlight(flightId, "Oslo", 2));
        await grain.OpenRegistration(new OpenRegistration(flightId));

        // Act
        await grain.RegisterPassenger(new RegisterPassenger(flightId, passengerId, "Ada"));

        // Assert
        var summary = await grain.GetSummary();

        await Assert.That(summary.PassengerIds).Contains(passengerId);
    }

    [Test]
    public async Task Closed_registration_is_visible_in_summary()
    {
        // Arrange
        var grain = NewFlight(out var flightId);

        await grain.Create(new CreateFlight(flightId, "Oslo", 2));
        await grain.OpenRegistration(new OpenRegistration(flightId));

        // Act
        await grain.CloseRegistration(new CloseRegistration(flightId));

        // Assert
        var summary = await grain.GetSummary();

        await Assert.That(summary.Registration).IsEqualTo(RegistrationStatus.Closed);
    }

    [Test]
    public async Task Domain_rules_still_apply_inside_the_grain()
    {
        // Arrange
        var grain = NewFlight(out var flightId);
        await grain.Create(new CreateFlight(flightId, "Oslo", 2));

        // Act / Assert
        await Assert.That(() => grain.AssignSeat(new AssignSeat(flightId, Guid.NewGuid(), "1A")))
            .Throws<CommandRejectedException>();
    }
}