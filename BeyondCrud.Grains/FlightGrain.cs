using BeyondCrud.Domain;
using BeyondCrud.Domain.Commands;

namespace BeyondCrud.Grains;

public class FlightGrain : Grain, IFlightGrain
{
    private readonly Flight _flight = new();

    public Task Create(CreateFlight command)
        => Execute(() => _flight.Create(command));

    public Task OpenRegistration(OpenRegistration command)
        => Execute(() => _flight.OpenRegistration(command));

    public Task RegisterPassenger(RegisterPassenger command)
    {
        // TODO: hand the command to the aggregate (see Execute)
        throw new NotImplementedException();
    }

    public Task CloseRegistration(CloseRegistration command)
    {
        // TODO: hand the command to the aggregate (see Execute)
        throw new NotImplementedException();
    }

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

    private static Task Execute(Action handle)
    {
        try
        {
            handle();
            return Task.CompletedTask;
        }
        catch (DomainException exception)
        {
            throw new CommandRejectedException(exception.Message);
        }
    }
}