using Application.Interfaces.Repositories.Logistics.Shipment;
using MediatR;

namespace Application.Features.Logistics.Commands.UpdateTrackingNumberCommand;

public class UpdateTrackingNumberCommandHandler(
    IShipmentReadRepository readRepository,
    IShipmentUpdateRepository updateRepository) : IRequestHandler<UpdateTrackingNumberCommand, bool>
{
    public async Task<bool> Handle(UpdateTrackingNumberCommand request, CancellationToken cancellationToken)
    {
        var order = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (order == null)
            return false;
        order.TrackingNumber = request.TrackingNumber;
        updateRepository.Update(order);
        return true;
    }
}
