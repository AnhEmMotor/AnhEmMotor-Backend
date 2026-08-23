using Application.Interfaces.Repositories.Logistics.Shipment;
using MediatR;

namespace Application.Features.Logistics.Commands.UpdateParcelStatusCommand;

public class UpdateParcelStatusCommandHandler(
    IShipmentReadRepository readRepository,
    IShipmentUpdateRepository updateRepository) : IRequestHandler<UpdateParcelStatusCommand, bool>
{
    public async Task<bool> Handle(UpdateParcelStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (order == null)
            return false;
        order.Status = request.NewStatus;
        order.DeliveredAt = request.NewStatus == Domain.Enums.ParcelDeliveryStatus.Completed
            ? DateTimeOffset.UtcNow
            : order.DeliveredAt;
        updateRepository.Update(order);
        return true;
    }
}
