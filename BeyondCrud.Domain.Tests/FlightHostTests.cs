using BeyondCrud.Domain.Commands;

namespace BeyondCrud.Domain.Tests;

public class FlightHostTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    [Test]
    public async Task Messages_are_handled_one_at_a_time()
    {
        // Arrange
        var host = new FlightHost();
        var running = 0;
        var maxRunning = 0;

        // Act
        var replies = Enumerable.Range(0, 100)
            .Select(_ => host.Tell(_ =>
            {
                var now = Interlocked.Increment(ref running);
                InterlockedMax(ref maxRunning, now);
                Thread.Sleep(1);
                Interlocked.Decrement(ref running);
            }));

        await Task.WhenAll(replies).WaitAsync(Timeout);

        // Assert
        await Assert.That(maxRunning).IsEqualTo(1);
    }

    [Test]
    public async Task Concurrent_registrations_are_all_applied()
    {
        // Arrange
        var flightId = Guid.NewGuid();
        var host = new FlightHost();

        await host.Tell(flight => flight.Create(new CreateFlight(flightId, "Oslo", 100))).WaitAsync(Timeout);
        await host.Tell(flight => flight.OpenRegistration(new OpenRegistration(flightId))).WaitAsync(Timeout);

        // Act
        var registrations = Enumerable.Range(0, 100)
            .Select(i => Task.Run(() => host.Tell(flight =>
                flight.RegisterPassenger(new RegisterPassenger(flightId, Guid.NewGuid(), $"Passenger {i}")))));

        await Task.WhenAll(registrations).WaitAsync(Timeout);

        // Assert
        var registered = await host.Ask(flight => flight.PassengerIds.Count).WaitAsync(Timeout);

        await Assert.That(registered).IsEqualTo(100);
    }

    private static void InterlockedMax(ref int target, int value)
    {
        int current;
        do
        {
            current = target;
            if (value <= current) return;
        } while (Interlocked.CompareExchange(ref target, value, current) != current);
    }
}