using BeyondCrud.Domain;

namespace BeyondCrud.Projections;

public record FlightListItem(
    Guid FlightId,
    string Destination,
    int Capacity,
    RegistrationStatus Registration,
    IReadOnlyList<string> Passengers);