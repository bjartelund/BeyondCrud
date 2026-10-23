namespace BeyondCrud.Domain.Events;

public record PassengerRegistered(Guid PassengerId, string Name) : IDomainEvent;