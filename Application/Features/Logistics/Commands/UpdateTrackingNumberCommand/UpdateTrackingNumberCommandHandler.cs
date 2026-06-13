using Application.Interfaces.Repositories.ParcelDeliveryOrder;
using MediatR;

namespace Application.Features.Logistics.Commands.UpdateTrackingNumberCommand;

public class UpdateTrackingNumberCommandHandler(
    IParcelDeliveryOrderReadRepository readRepository,
    IParcelDeliveryOrderUpdateRepository updateRepository) : IRequestHandler<UpdateTrackingNumberCommand, bool>
{
    public async Task<bool> Handle(UpdateTrackingNumberCommand request, CancellationToken cancellationToken)
    {
        var order = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (order == null) return false;

        order.TrackingNumber = request.TrackingNumber;
        updateRepository.Update(order);
        return true;
    }
}
