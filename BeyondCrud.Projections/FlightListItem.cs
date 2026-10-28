using BeyondCrud.Domain;

namespace BeyondCrud.Projections;

public record FlightListItem(
    Guid FlightId,
    string Destination,
    int Capacity,
    RegistrationStatus Registration,
    IReadOnlyList<FlightPassenger> Passengers);

public record FlightPassenger(Guid PassengerId, string Name, string? Seat, bool CheckedIn);