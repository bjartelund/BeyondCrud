namespace BeyondCrud.Domain.Events;

public record PassengerRegistered(Guid FlightId, Guid PassengerId, string Name) : IDomainEvent;
