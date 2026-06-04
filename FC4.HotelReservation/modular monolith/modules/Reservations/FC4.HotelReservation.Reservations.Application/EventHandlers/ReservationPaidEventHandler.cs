using FC4.HotelReservation.Reservations.Domain.Enums;
using FC4.HotelReservation.Reservations.Domain.Events;
using FC4.HotelReservation.Reservations.Events.IntegrationEvents;
using MassTransit;
using MediatR;

namespace FC4.HotelReservation.Reservations.Application.EventHandlers;

public class ReservationPaidEventHandler(IPublishEndpoint publishEndpoint)
    : INotificationHandler<ReservationPaidEvent>
{
    public Task Handle(ReservationPaidEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new ReservationStatusChanged(
            notification.ReservationId,
            ReservationStatus.Paid.ToString("G"));
        return publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
