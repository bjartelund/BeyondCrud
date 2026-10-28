using BeyondCrud.Domain;
using BeyondCrud.Domain.Commands;
using BeyondCrud.Infrastructure;

namespace BeyondCrud.Grains;

public class FlightGrain(IEventStore eventStore) : Grain, IFlightGrain
{
    private Flight _flight = new();
    private long _version = -1;

    private string StreamName => $"flight-{this.GetPrimaryKey()}";

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        // TODO: read the flight's stream, rebuild _flight from it and remember the stream version
        await Task.CompletedTask;
    }

    public Task Create(CreateFlight command)
        => Execute(() => _flight.Create(command));

    public Task OpenRegistration(OpenRegistration command)
        => Execute(() => _flight.OpenRegistration(command));

    public Task RegisterPassenger(RegisterPassenger command)
        => Execute(() => _flight.RegisterPassenger(command));

    public Task CloseRegistration(CloseRegistration command)
        => Execute(() => _flight.CloseRegistration(command));

    public Task AssignSeat(AssignSeat command)
        => Execute(() => _flight.AssignSeat(command));

    public Task<FlightSummary> GetSummary()
        => Task.FromResult(
            new FlightSummary(
                _flight.Id,
                _flight.Destination,
                _flight.Capacity,
                _flight.Registration,
                _flight.PassengerIds.ToArray(),
                new Dictionary<string, Guid>(_flight.Seats)));

    private async Task Execute(Action handle)
    {
        try
        {
            handle();
        }
        catch (DomainException exception)
        {
            throw new CommandRejectedException(exception.Message);
        }

        // TODO: append the new events to the stream (expecting _version), advance _version and clear them
        await Task.CompletedTask;
    }
}