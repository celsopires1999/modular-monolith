using FC4.HotelReservation.Payments.Events.IntegrationEvents;
using FC4.HotelReservation.Reservations.Application.Commands.ProcessPaymentStatus;
using MassTransit;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FC4.HotelReservation.Reservations.Consumers.Consumers;

public class PaymentStatusChangedConsumer(IServiceProvider provider, ILogger<PaymentStatusChangedConsumer> logger) : IConsumer<PaymentStatusChanged>
{
    public async Task Consume(ConsumeContext<PaymentStatusChanged> context)
    {
        logger.LogInformation("PaymentStatusChangedConsumer invoked for PaymentId={PaymentId}, ReservationId={ReservationId}, Status={Status}",
            context.Message.PaymentId, context.Message.ReservationId, context.Message.PaymentStatus);

        try
        {
            using var scope = provider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(new ProcessPaymentStatusCommand(
                    context.Message.PaymentId,
                    context.Message.ReservationId,
                    (PaymentStatus)context.Message.PaymentStatus),
                context.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing PaymentStatusChanged message for PaymentId={PaymentId}",
                context.Message.PaymentId);
            throw;
        }
    }
}