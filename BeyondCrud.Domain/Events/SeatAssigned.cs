namespace BeyondCrud.Domain.Events;

public record SeatAssigned(Guid FlightId, Guid PassengerId, string Seat) : IDomainEvent;
