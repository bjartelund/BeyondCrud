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
                readModel.Update(e.FlightId, flight => flight with { Passengers = [..flight.Passengers, e.Name] });
                break;

            case RegistrationClosed e:
                readModel.Update(e.FlightId, flight => flight with { Registration = RegistrationStatus.Closed });
                break;
        }
    }
}