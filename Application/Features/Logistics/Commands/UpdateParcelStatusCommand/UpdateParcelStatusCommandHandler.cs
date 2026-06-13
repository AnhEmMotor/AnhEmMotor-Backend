using Application.Interfaces.Repositories.ParcelDeliveryOrder;
using MediatR;

namespace Application.Features.Logistics.Commands.ProcessFulfillment;

public class UpdateParcelStatusCommandHandler(
    IParcelDeliveryOrderReadRepository readRepository,
    IParcelDeliveryOrderUpdateRepository updateRepository) : IRequestHandler<UpdateParcelStatusCommand, bool>
{
    public async Task<bool> Handle(UpdateParcelStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (order == null) return false;

        order.Status = request.NewStatus;
        updateRepository.Update(order);
        return true;
    }
}
