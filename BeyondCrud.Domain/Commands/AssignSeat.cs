namespace BeyondCrud.Domain.Commands;

public record AssignSeat(Guid FlightId, Guid PassengerId, string Seat);
