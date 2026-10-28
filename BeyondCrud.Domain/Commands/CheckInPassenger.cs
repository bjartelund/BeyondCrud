namespace BeyondCrud.Domain.Commands;

public record CheckInPassenger(Guid FlightId, Guid PassengerId);