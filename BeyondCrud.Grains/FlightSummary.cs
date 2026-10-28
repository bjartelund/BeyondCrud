using BeyondCrud.Domain;

namespace BeyondCrud.Grains;

public record FlightSummary(
    Guid FlightId,
    string Destination,
    int Capacity,
    RegistrationStatus Registration,
    IReadOnlyList<Guid> PassengerIds,
    IReadOnlyDictionary<string, Guid> Seats);