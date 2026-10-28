using BeyondCrud.Domain;
using BeyondCrud.Domain.Events;

namespace BeyondCrud.Projections;

public sealed class FlightListProjection(FlightListReadModel readModel)
{
    public void Apply(IDomainEvent @event)
    {
        switch (@event)
        {
            case FlightCreated e:
                readModel.Set(new FlightListItem(e.FlightId, e.Destination, e.Capacity, RegistrationStatus.NotOpen, []));
                break;

            case RegistrationOpened e:
                readModel.Update(e.FlightId, flight => flight with { Registration = RegistrationStatus.Open });
                break;

            case PassengerRegistered e:
                readModel.Update(
                    e.FlightId,
                    flight => flight with { Passengers = [..flight.Passengers, new FlightPassenger(e.PassengerId, e.Name, null, false)] });
                break;

            case RegistrationClosed e:
                readModel.Update(e.FlightId, flight => flight with { Registration = RegistrationStatus.Closed });
                break;

            case SeatAssigned e:
                UpdatePassenger(e.FlightId, e.PassengerId, passenger => passenger with { Seat = e.Seat });
                break;

            case PassengerCheckedIn e:
                UpdatePassenger(e.FlightId, e.PassengerId, passenger => passenger with { CheckedIn = true });
                break;
        }
    }

    private void UpdatePassenger(Guid flightId, Guid passengerId, Func<FlightPassenger, FlightPassenger> change)
        => readModel.Update(
            flightId,
            flight => flight with
            {
                Passengers = flight.Passengers.Select(p => p.PassengerId == passengerId ? change(p) : p).ToArray()
            });
}