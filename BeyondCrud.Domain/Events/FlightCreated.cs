namespace BeyondCrud.Domain.Events;

public record FlightCreated(Guid FlightId, string Destination, int Capacity) : IDomainEvent;
