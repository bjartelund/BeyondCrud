namespace BeyondCrud.Domain.Events;

public record RegistrationClosed(Guid FlightId) : IDomainEvent;
