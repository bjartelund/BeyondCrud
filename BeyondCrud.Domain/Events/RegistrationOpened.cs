namespace BeyondCrud.Domain.Events;

public record RegistrationOpened(Guid FlightId) : IDomainEvent;
