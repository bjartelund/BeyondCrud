using BeyondCrud.Domain.Commands;

namespace BeyondCrud.Grains;

public interface IFlightGrain : IGrainWithGuidKey
{
    Task Create(CreateFlight command);

    Task OpenRegistration(OpenRegistration command);

    Task RegisterPassenger(RegisterPassenger command);

    Task CloseRegistration(CloseRegistration command);

    Task AssignSeat(AssignSeat command);

    Task<FlightSummary> GetSummary();
}