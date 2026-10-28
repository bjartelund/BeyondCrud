namespace BeyondCrud.Domain.Commands;

public record RegisterPassenger(Guid FlightId, Guid PassengerId, string Name);
