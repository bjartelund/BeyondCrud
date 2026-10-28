namespace BeyondCrud.Domain.Commands;

public record CreateFlight(Guid FlightId, string Destination, int Capacity);
