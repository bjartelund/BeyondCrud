namespace BeyondCrud.Domain.Events;

public record PassengerCheckedIn(Guid FlightId, Guid PassengerId) : IDomainEvent;